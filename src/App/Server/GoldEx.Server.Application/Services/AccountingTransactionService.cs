using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.InventoryStocks;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.MeltingBatches;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class AccountingTransactionService(
    ITransactionRepository repository,
    ICustomerRepository customerRepository,
    IInvoiceRepository invoiceRepository,
    IPriceUnitRepository priceUnitRepository,
    IFinancialAccountRepository financialAccountRepository,
    IMeltingBatchRepository meltingBatchRepository,
    IProductRepository productRepository,
    IInventoryStockRepository inventoryStockRepository,
    ILedgerAccountRepository ledgerAccountRepository,
    ICoinRepository coinRepository,
    IPriceService priceService,
    ICoinService coinService,
    IServerLedgerAccountService ledgerAccountService)
    : IAccountingTransactionService
{
    private static DateTime ComposePostingDate(DateOnly businessDate, DateTime createdAt, long tickOffset = 0)
        => businessDate.ToDateTime(TimeOnly.FromTimeSpan(createdAt.TimeOfDay)).AddTicks(tickOffset);

    private static DateTime ComposePaymentPostingDate(DateTime paymentDateTime, DateTime paymentCreatedAt, long tickOffset, long innerIndex)
    {
        var time = paymentDateTime.TimeOfDay == TimeSpan.Zero ? paymentCreatedAt.TimeOfDay : paymentDateTime.TimeOfDay;
        var basePosting = new DateTime(paymentDateTime.Year, paymentDateTime.Month, paymentDateTime.Day,
            time.Hours, time.Minutes, time.Seconds, time.Milliseconds, paymentDateTime.Kind);
        return basePosting.AddTicks(tickOffset + innerIndex);
    }

    private readonly record struct TransactionSignatureFull(
        TransactionType Type,
        LedgerAccountId LedgerId,
        PriceUnitId PriceUnitId,
        decimal Amount,
        decimal? ExchangeRate,
        InvoiceId? InvoiceId,
        InvoicePaymentId? InvoicePaymentId,
        PaymentVoucherId? PaymentVoucherId,
        MeltingBatchId? MeltingBatchId
    );

    private readonly record struct TransactionSignatureMatch(
        TransactionType Type,
        LedgerAccountId LedgerId,
        PriceUnitId PriceUnitId,
        decimal Amount,
        decimal? ExchangeRate,
        InvoiceId? InvoiceId,
        InvoicePaymentId? InvoicePaymentId,
        PaymentVoucherId? PaymentVoucherId,
        MeltingBatchId? MeltingBatchId
    );

    private static TransactionSignatureFull SigFull(Transaction t) =>
        new(t.TransactionType, t.LedgerAccountId, t.PriceUnitId, Math.Round(t.Amount, 4), t.ExchangeRate.HasValue ? Math.Round(t.ExchangeRate.Value, 8) : null,
            t.InvoiceId, t.InvoicePaymentId, t.PaymentVoucherId, t.MeltingBatchId);

    private static TransactionSignatureMatch SigMatch(Transaction t) =>
        new(t.TransactionType, t.LedgerAccountId, t.PriceUnitId, Math.Round(t.Amount, 4), t.ExchangeRate.HasValue ? Math.Round(t.ExchangeRate.Value, 8) : null,
            t.InvoiceId, t.InvoicePaymentId, t.PaymentVoucherId, t.MeltingBatchId);

    private static TransactionType Invert(TransactionType t) =>
        t == TransactionType.Debit ? TransactionType.Credit : TransactionType.Debit;

    private static string BuildReversalDescription(Transaction t) => $"برگشت: {t.Description}";

    public async Task SetTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        var tx = await BuildTransactionsForInvoiceAsync(invoice, postingTickOffset: 0, cancellationToken);
        if (tx.Count > 0)
            await repository.CreateRangeAsync(tx, cancellationToken);
    }

    public async Task ReplaceTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        // Active فعلی
        var active = await GetActiveTransactionsForInvoiceAsync(invoice, includePayments: true, cancellationToken);

        // Preview بدون آفست برای قیاس
        var preview0 = await BuildTransactionsForInvoiceAsync(invoice, postingTickOffset: 0, cancellationToken);

        var preview0FullSet = preview0.Select(SigFull).ToHashSet();

        // باید برگردند (در قدیمی هستند ولی در جدید نیستند)
        var toReverse = active.Where(t => !preview0FullSet.Contains(SigFull(t))).ToList();

        // تطبیق بدون تاریخ برای تشخیص اقلام جدید
        var activeMatchSet = active.Select(SigMatch).ToHashSet();
        var preview0MatchSet = preview0.Select(SigMatch).ToHashSet();
        var toPostMatchSet = preview0MatchSet.Except(activeMatchSet).ToHashSet();

        if (toReverse.Count == 0 && toPostMatchSet.Count == 0) return;

        // 1) Reverse: با PostingDate = original.PostingDate + 1 tick
        if (toReverse.Count > 0)
        {
            var reversalGroupId = Guid.NewGuid();
            var reversals = BuildReversalTransactions(toReverse, reversalGroupId);
            await repository.CreateRangeAsync(reversals, cancellationToken);
        }

        // 2) Post: پیش‌نمایش با آفست 2 tick بساز و فقط آیتم‌های جدید را از آن بردار
        if (toPostMatchSet.Count > 0)
        {
            var preview2 = await BuildTransactionsForInvoiceAsync(invoice, postingTickOffset: 2, cancellationToken);
            var preview2ToPost = preview2.Where(t => toPostMatchSet.Contains(SigMatch(t))).ToList();

            if (preview2ToPost.Count > 0)
                await repository.CreateRangeAsync(preview2ToPost, cancellationToken);
        }
    }

    // برگشت‌ها: PostingDate = original.PostingDate + 1 tick (تا قبل از ثبت مجدد نمایش داده شوند)
    private static List<Transaction> BuildReversalTransactions(IEnumerable<Transaction> originals, Guid reversalGroupId)
    {
        var list = new List<Transaction>();

        foreach (var t in originals)
        {
            var reversedType = Invert(t.TransactionType);
            var desc = BuildReversalDescription(t);
            var reversalPostingDate = t.PostingDate.AddTicks(1); // کلید ترتیب

            Transaction rev;

            if (t.PaymentVoucherId is not null)
            {
                rev = Transaction.CreateForPaymentVoucher(
                    desc, t.Amount, t.ExchangeRate, reversalGroupId, reversedType,
                    t.LedgerAccountId, t.PriceUnitId, t.PaymentVoucherId!.Value, reversalPostingDate);
            }
            else if (t.MeltingBatchId is not null)
            {
                rev = Transaction.CreateForMeltingBatch(
                    description: desc,
                    amount: t.Amount,
                    exchangeRate: t.ExchangeRate,
                    baseCurrencyAmount: t.BaseCurrencyAmount,
                    transactionType: reversedType,
                    groupId: reversalGroupId,
                    priceUnitId: t.PriceUnitId,
                    ledgerAccountId: t.LedgerAccountId,
                    invoiceId: t.InvoiceId ?? default,
                    meltingBatchId: t.MeltingBatchId!.Value,
                    postingDate: reversalPostingDate
                );
            }
            else if (t.InvoicePaymentId is not null)
            {
                rev = Transaction.CreateForInvoicePayment(
                    desc, t.Amount, t.ExchangeRate, reversalGroupId, reversedType,
                    t.LedgerAccountId, t.PriceUnitId, t.InvoiceId ?? default, t.InvoicePaymentId!.Value, reversalPostingDate);
            }
            else if (t.InvoiceId is not null)
            {
                rev = Transaction.CreateForInvoice(
                    desc, t.Amount, t.ExchangeRate, reversalGroupId, reversedType,
                    t.LedgerAccountId, t.PriceUnitId, t.InvoiceId!.Value, reversalPostingDate);
            }
            else
            {
                continue;
            }

            rev.MarkAsReversalOf(t.Id);
            list.Add(rev);
        }

        return list;
    }

    private async Task<List<Transaction>> GetActiveTransactionsForInvoiceAsync(Invoice invoice, bool includePayments, CancellationToken ct)
    {
        var all = await repository
            .Get(new TransactionsByInvoiceIdSpecification(invoice.Id, includePayments))
            .AsNoTracking()
            .ToListAsync(ct);

        var reversedOriginalIds = all
            .Where(x => x.ReverseTransactionId != null)
            .Select(x => x.ReverseTransactionId!.Value)
            .ToHashSet();

        return all
            .Where(x => x.ReverseTransactionId == null && !reversedOriginalIds.Contains(x.Id))
            .ToList();
    }

    private async Task<List<Transaction>> GetActiveTransactionsForVoucherAsync(PaymentVoucher voucher, CancellationToken ct)
    {
        var all = await repository
            .Get(new TransactionsByPaymentVoucherIdSpecification(voucher.Id))
            .AsNoTracking()
            .ToListAsync(ct);

        var reversedOriginalIds = all
            .Where(x => x.ReverseTransactionId != null)
            .Select(x => x.ReverseTransactionId!.Value)
            .ToHashSet();

        return all
            .Where(x => x.ReverseTransactionId == null && !reversedOriginalIds.Contains(x.Id))
            .ToList();
    }

    // Builder: پارامتر postingTickOffset برای کنترل ترتیب (۰ برای عادی، ۲ برای ثبت مجدد)
    private async Task<List<Transaction>> BuildTransactionsForInvoiceAsync(Invoice invoice, long postingTickOffset = 0, CancellationToken cancellationToken = default)
    {
        var transactions = new List<Transaction>();

        if (invoice is { TotalAmountWithDiscountsAndExtraCosts: 0, TotalPaidAmount: 0 })
            return transactions;

        var customer = await customerRepository.Get(new CustomersByIdSpecification(invoice.CustomerId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Customer {invoice.CustomerId.Value} not found.");

        var basePriceUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Default price unit not found.");

        // پایه زمان: تاریخ فاکتور + زمان ایجاد فاکتور
        var basePosting = ComposePostingDate(invoice.InvoiceDate, invoice.CreatedAt, postingTickOffset);

        // درون‌سندی: برای خطوط مختلف یک ترتیب ثابت بدهیم
        long lineTick = 0;
        DateTime NextLine() => basePosting.AddTicks(lineTick++);

        if (invoice.TotalAmountWithDiscountsAndExtraCosts != 0)
        {
            var invoiceGroupId = Guid.NewGuid();
            switch (invoice.InvoiceType)
            {
                case InvoiceType.Sell:
                    {
                        var customerLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            customer.Id, invoice.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                        var salesRevenueLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.SalesRevenue))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Sales Revenue ledger account not found.");

                        var discountsLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.SalesDiscounts))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Sales Discounts ledger account not found.");

                        var extraChargesLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.AdditionalChargesRevenue))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Additional Charges Revenue ledger account not found.");

                        foreach (var instantProduct in invoice.ProductItems.Where(x => x.IsInstantProduct))
                        {
                            var manual = await CreateTransactionForManualEntryAsync(
                                instantProduct.Product, null, null,
                                instantProduct.CostPrice!.Value, instantProduct.CostPriceExchangeRate, instantProduct.CostPriceUnitId,
                                invoice.Id, NextLine(), cancellationToken);
                            transactions.AddRange(manual);
                        }

                        // COGS برای کالا
                        decimal totalCostOfGoods = 0;
                        foreach (var saleItem in invoice.ProductItems)
                        {
                            var purchaseInvoice = await invoiceRepository
                                .Get(new InvoicesByProductIdSpecification(saleItem.ProductId))
                                .FirstOrDefaultAsync(cancellationToken)
                                    ?? throw new NotFoundException($"Purchase invoice for product {saleItem.ProductId.Value} not found.");

                            var purchaseItem = purchaseInvoice.ProductItems
                                .FirstOrDefault(i => i.ProductId == saleItem.ProductId)
                                    ?? throw new NotFoundException($"Purchase invoice item for product {saleItem.ProductId.Value} not found.");

                            var purchaseItemBaseAmount = purchaseItem.ItemFinalAmount * (purchaseInvoice.ExchangeRate ?? 1);
                            var unitCost = purchaseItem.TotalWeight == 0 ? 0 : purchaseItemBaseAmount / purchaseItem.TotalWeight;
                            totalCostOfGoods += unitCost * saleItem.TotalWeight;
                        }

                        if (totalCostOfGoods > 0)
                        {
                            var cogsGroupId = Guid.NewGuid();

                            var cogsLedger = await ledgerAccountRepository
                                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.CostOfGoodsSold))
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Cost of Goods Sold ledger account not found.");

                            var inventoryLedger = await ledgerAccountRepository
                                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Inventory))
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Inventory ledger account not found.");

                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForCostOfGoodsSold(invoice),
                                totalCostOfGoods, null, cogsGroupId, TransactionType.Debit,
                                cogsLedger.Id, basePriceUnit.Id, invoice.Id, NextLine()));

                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForInvoiceInventoryExit(invoice),
                                totalCostOfGoods, null, cogsGroupId, TransactionType.Credit,
                                inventoryLedger.Id, basePriceUnit.Id, invoice.Id, NextLine()));
                        }

                        // فروش ارز: خروج از Ledger ارزی با میانگین بهای دفتری + سود/زیان تسعیر
                        var currencyLinesTotal = 0m;
                        if (invoice.CurrencyItems.Any())
                        {
                            var gainLossLedger = await ledgerAccountRepository
                                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.ExchangeGainLoss))
                                .FirstOrDefaultAsync(cancellationToken)
                                                 ?? throw new NotFoundException("Exchange Gain/Loss ledger account not found.");

                            foreach (var ci in invoice.CurrencyItems)
                            {
                                currencyLinesTotal += ci.ItemFinalAmount;

                                if (!ci.FinancialAccountId.HasValue)
                                    throw new NotFoundException("برای فروش ارز، انتخاب حساب مالی الزامی است.");

                                var fin = await financialAccountRepository
                                    .Get(new FinancialAccountsByIdSpecification(ci.FinancialAccountId.Value))
                                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("حساب مالی یافت نشد.");

                                if (!fin.LedgerAccountId.HasValue)
                                    throw new NotFoundException("حساب دفتری متناظر برای حساب مالی موجود نیست.");

                                var ledgerId = fin.LedgerAccountId.Value;

                                // میانگین بهای حساب دفتری ارز
                                var (_, _, avgRate) = await repository.GetLedgerPositionSummaryAsync(ledgerId, cancellationToken);

                                // خروج از حساب ارزی
                                transactions.Add(Transaction.CreateForInvoice(
                                    TransactionDescriptionBuilder.ForSaleCurrency(invoice.InvoiceNumber, ci.Currency?.Title ?? string.Empty),
                                    ci.Amount, avgRate, invoiceGroupId, TransactionType.Credit,
                                    ledgerId, ci.CurrencyId, invoice.Id, NextLine()));

                                // سود/زیان تسعیر (به ارز پایه)
                                var saleBase = ci.ItemFinalAmount * (invoice.ExchangeRate ?? 1);
                                var carryBase = ci.Amount * avgRate;
                                var diff = saleBase - carryBase;

                                if (diff > 0)
                                {
                                    transactions.Add(Transaction.CreateForInvoice(
                                        TransactionDescriptionBuilder.ForExchangeGain(invoice.InvoiceNumber, ci.Currency?.Title ?? string.Empty),
                                        diff, null, invoiceGroupId, TransactionType.Credit,
                                        gainLossLedger.Id, basePriceUnit.Id, invoice.Id, NextLine()));
                                }
                                else if (diff < 0)
                                {
                                    transactions.Add(Transaction.CreateForInvoice(
                                        TransactionDescriptionBuilder.ForExchangeLoss(invoice.InvoiceNumber, ci.Currency?.Title ?? string.Empty),
                                        Math.Abs(diff), null, invoiceGroupId, TransactionType.Debit,
                                        gainLossLedger.Id, basePriceUnit.Id, invoice.Id, NextLine()));
                                }
                            }
                        }

                        var coinLinesTotal = 0m;
                        if (invoice.CoinItems.Any())
                        {
                            var coinInventoryLedger = await ledgerAccountRepository
                                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.CoinInventory))
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Coin inventory ledger account not found.");

                            var coinSum = invoice.CoinItems.Sum(ci => ci.ItemFinalAmount);
                            if (coinSum > 0)
                            {
                                transactions.Add(Transaction.CreateForInvoice(
                                    TransactionDescriptionBuilder.ForSellCoin(invoice.InvoiceNumber),
                                    coinSum,
                                    invoice.ExchangeRate,
                                    invoiceGroupId,
                                    TransactionType.Credit,
                                    coinInventoryLedger.Id,
                                    invoice.PriceUnitId,
                                    invoice.Id, NextLine()));
                            }
                        }

                        // درآمد فروش: فقط اقلام غیرارزی (کالا + سکه + هزینه‌های اضافی)
                        var nonCurrencySalesAmount = invoice.TotalAmount - currencyLinesTotal - coinLinesTotal;
                        if (nonCurrencySalesAmount > 0)
                        {
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForSaleRevenue(invoice),
                                nonCurrencySalesAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Credit,
                                salesRevenueLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));
                        }

                        if (invoice.TotalExtraCostAmount > 0)
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForSaleExtraCharges(invoice),
                                invoice.TotalExtraCostAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Credit,
                                extraChargesLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));

                        if (invoice.TotalDiscountAmount > 0)
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForSaleDiscount(invoice,
                                    invoice.Discounts.Select(x => x.Description)),
                                invoice.TotalDiscountAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Debit,
                                discountsLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));

                        // دریافتنی مشتری (کل فاکتور)
                        transactions.Add(Transaction.CreateForInvoice(
                            TransactionDescriptionBuilder.ForSaleReceivable(invoice, customer),
                            invoice.TotalAmountWithDiscountsAndExtraCosts,
                            invoice.ExchangeRate,
                            invoiceGroupId,
                            TransactionType.Debit,
                            customerLedger.Id,
                            invoice.PriceUnitId,
                            invoice.Id, NextLine()));

                        break;
                    }
                case InvoiceType.Purchase:
                    {
                        var supplierLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(customer.Id,
                            invoice.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        var inventoryLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Inventory))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Inventory ledger account not found.");

                        var discountsLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.PurchaseDiscounts))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Purchase Discounts ledger account not found.");

                        var purchaseOverheadsLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.PurchaseOverheads))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Purchase Overheads account not found.");

                        foreach (var ci in invoice.CurrencyItems)
                        {
                            if (!ci.FinancialAccountId.HasValue)
                                throw new NotFoundException("برای آیتم ارزی، انتخاب «حساب مالی» الزامی است.");

                            var fin = await financialAccountRepository
                                .Get(new FinancialAccountsByIdSpecification(ci.FinancialAccountId.Value))
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Financial account {ci.FinancialAccountId.Value} not found.");

                            if (!fin.LedgerAccountId.HasValue)
                                throw new NotFoundException($"Financial account {fin.Id.Value} has no linked ledger account.");

                            var destLedger = await ledgerAccountRepository
                                .Get(new LedgerAccountsByIdSpecification(fin.LedgerAccountId.Value))
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Ledger account {fin.LedgerAccountId.Value} not found.");

                            // بدهکار به واحد ارز؛ نرخ = UnitPrice آیتم ارزی
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForPurchaseCurrencyEntry(fin.PriceUnit?.Title ?? string.Empty, invoice.InvoiceNumber),
                                ci.Amount,
                                ci.UnitPrice,
                                invoiceGroupId,
                                TransactionType.Debit,
                                destLedger.Id,
                                ci.CurrencyId,
                                invoice.Id, NextLine()));
                        }

                        // 2) Debit: سکه‌ها → CoinInventory
                        foreach (var coinItem in invoice.CoinItems)
                        {
                            var coin = await coinRepository
                                .Get(new CoinsByIdSpecification(coinItem.CoinId))
                                .FirstOrDefaultAsync(cancellationToken)
                                       ?? throw new NotFoundException($"Coin {coinItem.CoinId.Value} not found.");

                            var destLedger = await ledgerAccountRepository
                                .Get(new LedgerAccountsByTitleSpecification(LedgerAccountTitleBuilder.ForCoinAccount(coin.Title)))
                                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Coin '{coin.Title}' ledger account not found.");

                            // بدهکار به حساب دفتری سکه
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForPurchaseCoinEntry(invoice.InvoiceNumber, coin.Title),
                                coinItem.ItemFinalAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Debit,
                                destLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));
                        }

                        // 3) Debit: کالاها → Inventory
                        var productSum = invoice.ProductItems.Sum(pi => pi.ItemFinalAmount);
                        if (productSum > 0)
                        {
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForPurchaseInventoryEntry(invoice),
                                productSum,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Debit,
                                inventoryLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));
                        }

                        // 4) Debit: هزینه‌های اضافی خرید → حساب جداگانه
                        if (invoice.TotalExtraCostAmount > 0)
                        {
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForPurchaseOverheadCharges(invoice,
                                    invoice.ExtraCosts.Select(x => x.Description)),
                                invoice.TotalExtraCostAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Debit,
                                purchaseOverheadsLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));
                        }

                        // 5) Credit: حساب پرداختنی تأمین‌کننده
                        transactions.Add(Transaction.CreateForInvoice(
                            TransactionDescriptionBuilder.ForPurchasePayable(invoice, customer),
                            invoice.TotalAmountWithDiscountsAndExtraCosts,
                            invoice.ExchangeRate,
                            invoiceGroupId,
                            TransactionType.Credit,
                            supplierLedger.Id,
                            invoice.PriceUnitId,
                            invoice.Id, NextLine()));

                        // 6) Credit: تخفیفات خرید (در صورت وجود)
                        if (invoice.TotalDiscountAmount > 0)
                        {
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForPurchaseDiscount(invoice, customer,
                                    invoice.Discounts.Select(x => x.Description)),
                                invoice.TotalDiscountAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Credit,
                                discountsLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));
                        }

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Payments
        if (invoice.InvoicePayments != null)
        {
            foreach (var payment in invoice.InvoicePayments)
            {
                var paymentGroupId = Guid.NewGuid();

                long payLine = 0;
                DateTime NextPayLine()
                    => payment.PaymentDate.Date == invoice.InvoiceDate.ToDateTime(TimeOnly.MinValue).Date
                        ? basePosting.AddTicks(1000 + payLine++)
                        : ComposePaymentPostingDate(payment.PaymentDate, payment.CreatedAt, postingTickOffset, payLine++);

                // 1) پرداخت‌های مبتنی بر حساب مالی (نقدی / کارت / ... )
                if (payment.SourceFinancialAccountId.HasValue)
                {
                    var sourceFinancialAccount = await financialAccountRepository
                        .Get(new FinancialAccountsByIdSpecification(payment.SourceFinancialAccountId.Value))
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException($"Financial account {payment.SourceFinancialAccountId.Value} not found.");

                    if (!sourceFinancialAccount.LedgerAccountId.HasValue)
                        throw new NotFoundException($"Financial account {sourceFinancialAccount.Id.Value} has no linked ledger account.");

                    var sourceLedgerAccount = await ledgerAccountRepository
                        .Get(new LedgerAccountsByIdSpecification(sourceFinancialAccount.LedgerAccountId.Value))
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException($"Ledger account {sourceFinancialAccount.LedgerAccountId.Value} not found.");

                    // حساب طرف مقابل: مشتری/تأمین‌کننده
                    var counterpartyRole = invoice.InvoiceType == InvoiceType.Sell
                        ? LedgerAccountRole.Receivable
                        : LedgerAccountRole.Payable;

                    var counterpartyLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                        customer.Id, payment.PriceUnitId, counterpartyRole, cancellationToken);

                    var description = TransactionDescriptionBuilder.ForInvoicePayment(invoice, payment);

                    if (payment.PaymentSide == PaymentSide.Receive)
                    {
                        // دریافت وجه (از مشتری / از تأمین‌کننده)
                        // Debit: حساب مالی
                        // Credit: حساب دریافتنی / پرداختنی طرف حساب
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit,
                            sourceLedgerAccount.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit,
                            counterpartyLedger.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));
                    }
                    else // PaymentSide.Pay
                    {
                        // پرداخت وجه (به مشتری / به تأمین‌کننده)
                        // Debit: حساب دریافتنی / پرداختنی طرف حساب
                        // Credit: حساب مالی
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit,
                            counterpartyLedger.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit,
                            sourceLedgerAccount.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));
                    }
                }

                // 2) پرداخت با طلای آبشده یا طلای شکسته (موجودی طلا)
                else if (payment.PaymentType is PaymentType.MoltenGoldInventory or PaymentType.UsedGoldInventory)
                {
                    if (!payment.GoldFineness.HasValue)
                        throw new InvalidOperationException("Gold fineness is required for gold inventory payments.");

                    var goldLedger = await ledgerAccountRepository
                        .Get(new LedgerAccountsByTitleSpecification(
                            payment.PaymentType == PaymentType.MoltenGoldInventory
                                ? SystemLedgerAccounts.MoltenGoldInventory
                                : SystemLedgerAccounts.UsedProductInventory))
                        .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException("Gold inventory ledger account not found.");

                    var counterpartyRole = invoice.InvoiceType == InvoiceType.Sell
                        ? LedgerAccountRole.Receivable
                        : LedgerAccountRole.Payable;

                    var counterpartyLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                        customer.Id, payment.PriceUnitId, counterpartyRole, cancellationToken);

                    var description = TransactionDescriptionBuilder.ForGoldPayment(invoice, payment);

                    if (payment.PaymentSide == PaymentSide.Receive)
                    {
                        // دریافت طلا از مشتری/تأمین‌کننده
                        // Debit: موجودی طلا
                        // Credit: حساب دریافتنی / پرداختنی طرف حساب
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit,
                            goldLedger.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit,
                            counterpartyLedger.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));
                    }
                    else // PaymentSide.Pay
                    {
                        // پرداخت طلا به مشتری/تأمین‌کننده
                        // Debit: حساب دریافتنی / پرداختنی طرف حساب
                        // Credit: موجودی طلا
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit,
                            counterpartyLedger.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit,
                            goldLedger.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));
                    }
                }

                // 3) حواله‌کرد (CustomerTransfer)
                else if (payment.PaymentType == PaymentType.CustomerTransfer)
                {
                    if (payment.LedgerAccount is null)
                        throw new NotFoundException("Ledger account is required for CustomerTransfer payments.");

                    if (!payment.LedgerAccount.CustomerId.HasValue)
                        throw new InvalidOperationException("Endorser customer is required for CustomerTransfer payments.");

                    var endorserId = payment.LedgerAccount.CustomerId.Value;

                    if (invoice.InvoiceType == InvoiceType.Sell)
                    {
                        // فاکتور فروش: هم مشتری اصلی و هم حواله‌کرد هر دو Receivable هستند
                        var endorserLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            endorserId, payment.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                        var customerReceivableAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            invoice.CustomerId, payment.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                        var desc = TransactionDescriptionBuilder.ForInvoicePaymentByEndorser(
                            invoice, payment, endorserLedger.Customer!.FullName);

                        if (payment.PaymentSide == PaymentSide.Receive)
                        {
                            // دریافت به نفع ما: بدهکار کردن مشتری اصلی، بستانکار کردن حواله‌کرد
                            transactions.Add(Transaction.CreateForInvoicePayment(
                                desc,
                                payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                                TransactionType.Debit,
                                customerReceivableAccount.Id,
                                payment.PriceUnitId,
                                invoice.Id,
                                payment.Id,
                                NextPayLine()));

                            transactions.Add(Transaction.CreateForInvoicePayment(
                                desc,
                                payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                                TransactionType.Credit,
                                endorserLedger.Id,
                                payment.PriceUnitId,
                                invoice.Id,
                                payment.Id,
                                NextPayLine()));
                        }
                        else // PaymentSide.Pay
                        {
                            // پرداخت از سمت ما با حواله‌کرد در فروش به‌ندرت رخ می‌دهد؛
                            // تفسیر: بدهکار کردن حواله‌کرد، بستانکار کردن مشتری اصلی
                            transactions.Add(Transaction.CreateForInvoicePayment(
                                desc,
                                payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                                TransactionType.Debit,
                                endorserLedger.Id,
                                payment.PriceUnitId,
                                invoice.Id,
                                payment.Id,
                                NextPayLine()));

                            transactions.Add(Transaction.CreateForInvoicePayment(
                                desc,
                                payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                                TransactionType.Credit,
                                customerReceivableAccount.Id,
                                payment.PriceUnitId,
                                invoice.Id,
                                payment.Id,
                                NextPayLine()));
                        }
                    }
                    else // InvoiceType.Purchase
                    {
                        // فاکتور خرید: هر دو Payable هستند
                        var endorserLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            endorserId, payment.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        var customerPayableAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            invoice.CustomerId, payment.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        var desc = TransactionDescriptionBuilder.ForInvoicePaymentByEndorser(
                            invoice, payment, endorserLedger.Customer!.FullName);

                        if (payment.PaymentSide == PaymentSide.Pay)
                        {
                            // پرداخت از سمت ما به تأمین‌کننده با حواله‌کرد:
                            // Debit: حساب حواله‌کرد (بدهکار به او)
                            // Credit: حساب پرداختنی تأمین‌کننده
                            transactions.Add(Transaction.CreateForInvoicePayment(
                                desc,
                                payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                                TransactionType.Debit,
                                endorserLedger.Id,
                                payment.PriceUnitId,
                                invoice.Id,
                                payment.Id,
                                NextPayLine()));

                            transactions.Add(Transaction.CreateForInvoicePayment(
                                desc,
                                payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                                TransactionType.Credit,
                                customerPayableAccount.Id,
                                payment.PriceUnitId,
                                invoice.Id,
                                payment.Id,
                                NextPayLine()));
                        }
                        else // PaymentSide.Receive
                        {
                            // دریافت از تأمین‌کننده/حواله‌کرد در فاکتور خرید (کمتر معمول، ولی از نظر حسابداری مجاز)
                            // Debit: حساب پرداختنی تأمین‌کننده
                            // Credit: حساب حواله‌کرد
                            transactions.Add(Transaction.CreateForInvoicePayment(
                                desc,
                                payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                                TransactionType.Debit,
                                customerPayableAccount.Id,
                                payment.PriceUnitId,
                                invoice.Id,
                                payment.Id,
                                NextPayLine()));

                            transactions.Add(Transaction.CreateForInvoicePayment(
                                desc,
                                payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                                TransactionType.Credit,
                                endorserLedger.Id,
                                payment.PriceUnitId,
                                invoice.Id,
                                payment.Id,
                                NextPayLine()));
                        }
                    }
                }

                // 4) پرداخت فاکتور از محل سند پرداخت (PaymentVoucher)
                else if (payment is { PaymentVoucherId: not null, PaymentVoucher: not null })
                {
                    // در حال حاضر فقط برای فاکتور خرید طراحی شده
                    if (invoice.InvoiceType == InvoiceType.Purchase)
                    {
                        var supplierLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            customer.Id, invoice.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        var prepaymentLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.PrepaymentsToSuppliers))
                            .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new NotFoundException("Prepayments to Suppliers ledger account not found.");

                        var description = TransactionDescriptionBuilder.ForPaymentVoucher(payment.PaymentVoucher, invoice);

                        // استفاده از پیش‌پرداخت برای تسویه فاکتور خرید:
                        // Debit: حساب پرداختنی تأمین‌کننده
                        // Credit: پیش‌پرداخت به تأمین‌کننده
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.Amount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit,
                            supplierLedger.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.Amount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit,
                            prepaymentLedger.Id,
                            payment.PriceUnitId,
                            invoice.Id,
                            payment.Id,
                            NextPayLine()));
                    }
                }
            }
        }

        // Used products
        if (invoice.UsedProducts.Any())
        {
            long usedLineTick = 0;
            foreach (var usedProduct in invoice.UsedProducts)
            {
                var usedTx = await CreateTransactionForUsedProductsAsync(usedProduct, usedLineTick++, cancellationToken);
                transactions.AddRange(usedTx);
            }
        }

        return transactions;
    }

    private async Task<List<Transaction>> BuildTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, long postingTickOffset = 0, CancellationToken cancellationToken = default)
    {
        var transactions = new List<Transaction>();

        if (voucher.Amount == 0) return transactions;

        var groupId = Guid.NewGuid();

        var sourceFinancialAccount = await financialAccountRepository
            .Get(new FinancialAccountsByIdSpecification(voucher.SourceFinancialAccountId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Financial account {voucher.SourceFinancialAccountId.Value} not found.");

        if (!sourceFinancialAccount.LedgerAccountId.HasValue)
            throw new NotFoundException($"Financial account {sourceFinancialAccount.Id.Value} does not have a linked ledger account.");

        var creditLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByIdSpecification(sourceFinancialAccount.LedgerAccountId.Value))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Ledger account {sourceFinancialAccount.LedgerAccountId.Value} not found.");

        var voucherCustomer = await customerRepository
            .Get(new CustomersByIdSpecification(voucher.CustomerId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Customer {voucher.CustomerId.Value} not found.");

        LedgerAccount debitLedgerAccount;
        string description;

        switch (voucher.VoucherType)
        {
            case PaymentVoucherType.PrepaymentToSupplier:
                debitLedgerAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(voucher.CustomerId,
                    voucher.VoucherPriceUnitId, LedgerAccountRole.Payable, cancellationToken);
                description = TransactionDescriptionBuilder.ForPrepaymentToCustomer(voucher, voucherCustomer);
                break;

            case PaymentVoucherType.RefundToCustomer:
                debitLedgerAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(voucher.CustomerId,
                    voucher.VoucherPriceUnitId, LedgerAccountRole.Receivable, cancellationToken);
                description = TransactionDescriptionBuilder.ForRefundToCustomer(voucher, voucherCustomer);
                break;

            case PaymentVoucherType.ServiceFeePayment:
                debitLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.ServiceExpenses))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Service Expenses account not found.");
                description = TransactionDescriptionBuilder.ForServiceFeePayment(voucher, voucherCustomer);
                break;

            case PaymentVoucherType.PartnerLoan:
                debitLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.LoansToOthers))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("LoansToOthers account not found.");
                description = TransactionDescriptionBuilder.ForPartnerLoan(voucher, voucherCustomer);
                break;

            case PaymentVoucherType.OwnerDraw:
                debitLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.OwnerDraw))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Owner Draw account not found.");
                description = TransactionDescriptionBuilder.ForOwnerDraw(voucher, voucherCustomer);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(voucher.VoucherType));
        }

        var postingDate = voucher.PaymentDate.ToDateTime(TimeOnly.FromTimeSpan(voucher.CreatedAt.TimeOfDay)).AddTicks(postingTickOffset);

        transactions.Add(Transaction.CreateForPaymentVoucher(
            description,
            voucher.Amount,
            voucher.ExchangeRate,
            groupId,
            TransactionType.Debit,
            debitLedgerAccount.Id,
            voucher.VoucherPriceUnitId,
            voucher.Id, postingDate));

        transactions.Add(Transaction.CreateForPaymentVoucher(
            description,
            voucher.Amount,
            voucher.ExchangeRate,
            groupId,
            TransactionType.Credit,
            creditLedgerAccount.Id,
            voucher.VoucherPriceUnitId,
            voucher.Id, postingDate));

        return transactions;
    }

    public async Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default)
    {
        // Delta-based: هم برای Create و هم Update
        var active = await GetActiveTransactionsForVoucherAsync(voucher, cancellationToken);

        // اگر مبلغ صفر شد: فقط برگشت بده (Audit-safe)
        if (voucher.Amount == 0)
        {
            if (active.Count > 0)
            {
                var reversalGroupId = Guid.NewGuid();
                var reversals = BuildReversalTransactions(active, reversalGroupId);
                await repository.CreateRangeAsync(reversals, cancellationToken);
            }
            return;
        }

        // پیش‌نمایش بدون آفست
        var preview0 = await BuildTransactionsForPaymentVoucherAsync(voucher, postingTickOffset: 0, cancellationToken);

        if (active.Count == 0)
        {
            // سناریوی Create واقعی
            if (preview0.Count > 0)
                await repository.CreateRangeAsync(preview0, cancellationToken);
            return;
        }

        // Delta محاسبه شود
        var preview0FullSet = preview0.Select(SigFull).ToHashSet();
        var toReverse = active.Where(t => !preview0FullSet.Contains(SigFull(t))).ToList();

        var activeMatchSet = active.Select(SigMatch).ToHashSet();
        var preview0MatchSet = preview0.Select(SigMatch).ToHashSet();
        var toPostMatchSet = preview0MatchSet.Except(activeMatchSet).ToHashSet();

        if (toReverse.Count == 0 && toPostMatchSet.Count == 0)
            return;

        // 1) Reverse (posting = original + 1 tick)
        if (toReverse.Count > 0)
        {
            var reversalGroupId = Guid.NewGuid();
            var reversals = BuildReversalTransactions(toReverse, reversalGroupId);
            await repository.CreateRangeAsync(reversals, cancellationToken);
        }

        // 2) Post new (paymentDate + 2 ticks)
        if (toPostMatchSet.Count > 0)
        {
            var preview2 = await BuildTransactionsForPaymentVoucherAsync(voucher, postingTickOffset: 2, cancellationToken);
            var preview2ToPost = preview2.Where(t => toPostMatchSet.Contains(SigMatch(t))).ToList();

            if (preview2ToPost.Count > 0)
                await repository.CreateRangeAsync(preview2ToPost, cancellationToken);
        }
    }

    public Task ClearTransactionsForPaymentVoucherAsync(PaymentVoucher voucher,
        CancellationToken cancellationToken = default)
    {
        return repository.RemoveByPaymentVoucherIdAsync(voucher.Id, cancellationToken);
    }

    public async Task ClearTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        await repository.RemoveByInvoiceIdAsync(invoice.Id, cancellationToken);
        await repository.RemoveByInvoicePaymentIdsAsync(invoice.InvoicePayments?.Select(x => x.Id).ToList(),
            cancellationToken);
    }

    public async Task SetForMeltingBatchRequestAsync(MeltingBatchId meltingBatchId, List<ProductId> productIds,
        CancellationToken cancellationToken = default)
    {
        var inventoryLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Inventory))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Inventory ledger account not found.");

        var cogsLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.CostOfGoodsSold))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Cost of Goods Sold ledger account not found.");

        var meltingBatch = await meltingBatchRepository
            .Get(new MeltingBatchesByIdSpecification(meltingBatchId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Melting batch {meltingBatchId.Value} not found.");

        var products = await productRepository
            .Get(new ProductsByIdsSpecification(productIds))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
            throw new NotFoundException("One or more products not found.");

        var basePriceUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Default price unit not found.");

        var transactions = new List<Transaction>();

        var groupId = Guid.NewGuid();

        foreach (var product in products)
        {
            var invoice = await invoiceRepository
                .Get(new InvoicesByUsedProductIdSpecification(product.Id))
                .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new NotFoundException($"Invoice for product {product.Id.Value} not found.");

            var invoiceItem = invoice.UsedProducts.FirstOrDefault(x => x.ProductId == product.Id)
                              ?? throw new NotFoundException($"Used product item for product {product.Id.Value} not found in invoice {invoice.Id.Value}.");

            var amount = invoiceItem.ItemFinalAmount; // ارزش نهایی آیتم (قیمت خرید)
            var baseCurrencyAmount = amount * (invoice.ExchangeRate ?? 1); // معادل ارز پایه (ریال)

            // تراکنش Debit: ثبت در COGS (هزینه خروج برای محاسبه کسر ذوب)
            var debitTransaction = Transaction.CreateForMeltingBatch(
                description: TransactionDescriptionBuilder.ForMeltingBatchCogs(meltingBatch, product, invoice),
                amount: baseCurrencyAmount,
                exchangeRate: invoice.ExchangeRate,
                baseCurrencyAmount: baseCurrencyAmount,
                transactionType: TransactionType.Debit,
                groupId: groupId,
                priceUnitId: basePriceUnit.Id,
                ledgerAccountId: cogsLedgerAccount.Id,
                invoiceId: invoice.Id,
                meltingBatchId: meltingBatchId,
                postingDate: DateTime.Now);

            // تراکنش Credit: کاهش موجودی Inventory
            var creditTransaction = Transaction.CreateForMeltingBatch(
                description: TransactionDescriptionBuilder.ForMeltingBatchInventoryExit(meltingBatch, product, invoice),
                amount: baseCurrencyAmount,
                exchangeRate: invoice.ExchangeRate,
                baseCurrencyAmount: baseCurrencyAmount,
                transactionType: TransactionType.Credit,
                groupId: groupId,
                priceUnitId: basePriceUnit.Id,
                ledgerAccountId: inventoryLedgerAccount.Id,
                invoiceId: invoice.Id,
                meltingBatchId: meltingBatchId,
                postingDate: DateTime.Now);

            transactions.Add(debitTransaction);
            transactions.Add(creditTransaction);
        }

        if (transactions.Any())
        {
            await repository.CreateRangeAsync(transactions, cancellationToken);
        }
    }

    public async Task SetForMoltenGoldEntryAsync(MeltingBatch meltingBatch, CompleteMeltingRequestDto request, CancellationToken cancellationToken = default)
    {
        var moltenValue = CalculatorHelper.MoltenGold.Calculate(request.Weight, request.Fineness, request.GramPrice, null);

        // دریافت حساب‌ها
        var moltenInventoryAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.MoltenGoldInventory))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("MoltenGoldInventory ledger account not found.");

        var cogsLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.CostOfGoodsSold))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Cost of Goods Sold ledger account not found.");

        var priceUnit = await priceUnitRepository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(request.PriceUnitId)))
            .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException($"Price unit with id '{request.PriceUnitId}' not found.");

        var transactions = new List<Transaction>();
        var groupId = Guid.NewGuid();

        // تراکنش Debit: افزایش موجودی طلای آبشده
        var entryDebit = Transaction.CreateForMoltenGold(
            description: MoltenGoldDescriptionBuilder.BuildMoltenEntryDebit(request.Weight, request.Fineness, request.AssayNumber, meltingBatch.BatchNumber, meltingBatch.Assayer?.FullName),
            amount: moltenValue,
            exchangeRate: null,
            baseCurrencyAmount: moltenValue,
            transactionType: TransactionType.Debit,
            groupId: groupId,
            priceUnitId: priceUnit.Id,
            ledgerAccountId: moltenInventoryAccount.Id,
            meltingBatchId: meltingBatch.Id,
            postingDate: DateTime.Now);

        // تراکنش Credit: تسویه بهای تمام‌شده ذوب
        var entryCredit = Transaction.CreateForMoltenGold(
            description: MoltenGoldDescriptionBuilder.BuildMoltenEntryCredit(request.Weight, request.Fineness, request.AssayNumber, meltingBatch.BatchNumber, moltenValue, priceUnit.Title),
            amount: moltenValue,
            exchangeRate: null,
            baseCurrencyAmount: moltenValue,
            transactionType: TransactionType.Credit,
            groupId: groupId,
            priceUnitId: priceUnit.Id,
            ledgerAccountId: cogsLedgerAccount.Id,
            meltingBatchId: meltingBatch.Id,
            postingDate: DateTime.Now);

        transactions.Add(entryDebit);
        transactions.Add(entryCredit);

        // ثبت هزینه‌های جانبی ذوب
        if (request is { FeeAmount: > 0, FinancialAccountId: not null })
        {
            var expensesAccount = await ledgerAccountRepository
                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.OperatingExpenses))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("OperatingExpenses ledger account not found.");

            var sourceFinancialAccount = await financialAccountRepository
                                             .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(request.FinancialAccountId.Value)))
                                             .FirstOrDefaultAsync(cancellationToken)
                                         ?? throw new NotFoundException($"Financial account {request.FinancialAccountId.Value} not found.");

            if (!sourceFinancialAccount.LedgerAccountId.HasValue)
                throw new NotFoundException(
                    $"Financial account {sourceFinancialAccount.Id.Value} does not have a linked ledger account.");

            var sourceLedgerAccount = await ledgerAccountRepository
                                          .Get(new LedgerAccountsByIdSpecification(sourceFinancialAccount.LedgerAccountId.Value))
                                          .FirstOrDefaultAsync(cancellationToken)
                                      ?? throw new NotFoundException($"Ledger account {sourceFinancialAccount.LedgerAccountId.Value} not found.");

            var feeExchangeRate = request.FeeExchangeRate;
            var feeBaseAmount = request.FeeAmount.Value * (feeExchangeRate ?? 1);

            var feePriceUnitId = request.FeePriceUnitId ?? request.PriceUnitId;
            var feePriceUnit = await priceUnitRepository
                .Get(new PriceUnitsByIdSpecification(new PriceUnitId(feePriceUnitId)))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Fee price unit with id '{feePriceUnitId}' not found.");

            var feeGroupId = Guid.NewGuid();

            // تراکنش Debit: ثبت هزینه ری‌گیری
            var feeDebit = Transaction.CreateForMoltenGold(
                description: MoltenGoldDescriptionBuilder.BuildAssayFeeDebit(request.FeeAmount.Value, feePriceUnit.Title, feeExchangeRate, feeBaseAmount, priceUnit.Title, meltingBatch.BatchNumber, request.AssayNumber, meltingBatch.Assayer?.FullName),
                amount: request.FeeAmount.Value,
                exchangeRate: feeExchangeRate,
                baseCurrencyAmount: feeBaseAmount,
                transactionType: TransactionType.Debit,
                groupId: feeGroupId,
                priceUnitId: feePriceUnit.Id,
                ledgerAccountId: expensesAccount.Id,
                meltingBatchId: meltingBatch.Id,
                postingDate: DateTime.Now);

            // تراکنش Credit: پرداخت هزینه ری‌گیری
            var feeCredit = Transaction.CreateForMoltenGold(
                description: MoltenGoldDescriptionBuilder.BuildAssayFeeCredit(request.FeeAmount.Value, feePriceUnit.Title, feeExchangeRate, feeBaseAmount, priceUnit.Title, meltingBatch.BatchNumber),
                amount: request.FeeAmount.Value,
                exchangeRate: feeExchangeRate,
                baseCurrencyAmount: feeBaseAmount,
                transactionType: TransactionType.Credit,
                groupId: feeGroupId,
                priceUnitId: feePriceUnit.Id,
                ledgerAccountId: sourceLedgerAccount.Id,
                meltingBatchId: meltingBatch.Id,
                postingDate: DateTime.Now);

            transactions.Add(feeDebit);
            transactions.Add(feeCredit);
        }

        // ذخیره تراکنش‌ها
        await repository.CreateRangeAsync(transactions, cancellationToken);
    }

    private async Task<List<Transaction>> CreateTransactionForManualEntryAsync(Product? product, Coin? coin, PriceUnit? currency,
        decimal costPrice, decimal? costPriceExchangeRate, PriceUnitId? costPriceUnitId, InvoiceId triggeringInvoiceId, DateTime postingDate,
        CancellationToken cancellationToken = default)
    {
        if (costPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(costPrice),
                "Cost price must be greater than or equal to zero.");

        if (costPriceExchangeRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(costPriceExchangeRate),
                "Cost price exchange rate must be greater than zero if provided.");

        if (product is not null && (coin is not null || currency is not null))
            throw new ArgumentException("If product is provided, coin and currency must be null.");

        if (coin is not null && (product is not null || currency is not null))
            throw new ArgumentException("If coin is provided, product and currency must be null.");

        if (currency is not null && (product is not null || coin is not null))
            throw new ArgumentException("If currency is provided, product and coin must be null.");

        var transactions = new List<Transaction>();

        var groupId = Guid.NewGuid();

        var description = product != null
            ? TransactionDescriptionBuilder.ForManualProductEntry(product.Name)
            : coin != null
                ? TransactionDescriptionBuilder.ForManualCoinEntry(coin.Title)
                : currency != null
                    ? TransactionDescriptionBuilder.ForManualCurrencyEntry(currency.Title)
                    : throw new ArgumentException("At least one of product, coin, or currency must be provided.",
                        nameof(product));

        var basePriceUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Default price unit not found.");

        var openingBalanceLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.OpeningBalanceEquity))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Opening Balance Equity ledger account not found.");

        LedgerAccountId debitLedgerAccountId;

        if (product is not null)
        {
            var inventoryLedgerAccount = await ledgerAccountRepository
                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Inventory))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Inventory ledger account not found.");

            debitLedgerAccountId = inventoryLedgerAccount.Id;
        }
        else if (coin is not null)
        {
            var coinLedgerAccount = await ledgerAccountRepository
                .Get(new LedgerAccountsByTitleSpecification(LedgerAccountTitleBuilder.ForCoinAccount(coin.Title)))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Coin '{coin.Title}' ledger account not found.");

            debitLedgerAccountId = coinLedgerAccount.Id;
        }
        else if (currency is not null)
        {
            var currencyLedgerAccount = await ledgerAccountRepository
                .Get(new LedgerAccountsByTitleSpecification(LedgerAccountTitleBuilder.ForCurrencyAccount(currency.Title)))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Currency '{currency.Title}' ledger account not found.");

            debitLedgerAccountId = currencyLedgerAccount.Id;
        }
        else
        {
            throw new ArgumentException("At least one of product, coin, or currency must be provided.",
                nameof(product));
        }

        transactions.Add(Transaction.CreateForManualEntry(
            description,
            costPrice,
            costPriceExchangeRate,
            groupId,
            TransactionType.Debit,
            debitLedgerAccountId,
            costPriceUnitId ?? basePriceUnit.Id,
            triggeringInvoiceId,
            null,
            postingDate));

        transactions.Add(Transaction.CreateForManualEntry(
            description,
            costPrice,
            costPriceExchangeRate,
            groupId,
            TransactionType.Credit,
            openingBalanceLedgerAccount.Id,
            costPriceUnitId ?? basePriceUnit.Id,
            triggeringInvoiceId,
            null,
            postingDate));

        return transactions;
    }

    private async Task<List<Transaction>> CreateTransactionForUsedProductsAsync(InvoiceUsedProduct usedProduct, long ticks = 0, CancellationToken cancellationToken = default)
    {
        var customer = await customerRepository
                           .Get(new CustomersByIdSpecification(usedProduct.Invoice.CustomerId))
                           .AsNoTracking()
                           .FirstOrDefaultAsync(cancellationToken)
                       ?? throw new NotFoundException($"Customer {usedProduct.Invoice.CustomerId.Value} not found.");

        var customerReceivableLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
            usedProduct.Invoice.CustomerId,
            usedProduct.Invoice.PriceUnitId,
            LedgerAccountRole.Receivable,
            cancellationToken);

        var inventoryLedgerAccount = await ledgerAccountRepository
                                         .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Inventory))
                                         .FirstOrDefaultAsync(cancellationToken) ??
                                     throw new NotFoundException("Inventory ledger account not found.");

        var usedProductInventoryLedgerAccount = await ledgerAccountRepository
                                         .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.UsedProductInventory))
                                         .FirstOrDefaultAsync(cancellationToken) ??
                                     throw new NotFoundException("UsedProductInventory ledger account not found.");

        var debitLedgerAccountId = usedProduct.IsBroken ? usedProductInventoryLedgerAccount.Id : inventoryLedgerAccount.Id;

        var transactions = new List<Transaction>();
        var groupId = Guid.NewGuid();
        var description = TransactionDescriptionBuilder.ForUsedProductPurchase(usedProduct, customer);

        var postingDate = ComposePostingDate(usedProduct.Invoice.InvoiceDate, usedProduct.Invoice.CreatedAt, ticks + 200);

        transactions.Add(Transaction.CreateForInvoice(
            description,
            usedProduct.ItemFinalAmount,
            usedProduct.Invoice.ExchangeRate,
            groupId,
            TransactionType.Debit,
            debitLedgerAccountId,
            usedProduct.Invoice.PriceUnitId,
            usedProduct.InvoiceId,
            postingDate));

        transactions.Add(Transaction.CreateForInvoice(
            description,
            usedProduct.ItemFinalAmount,
            usedProduct.Invoice.ExchangeRate,
            groupId,
            TransactionType.Credit,
            customerReceivableLedger.Id,
            usedProduct.Invoice.PriceUnitId,
            usedProduct.InvoiceId,
            postingDate));

        return transactions;
    }

    public async Task CreateForInventoryEntryAsync(
        InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        Product product,
        CreateProductItemEntryRequest productItemEntryRequest,
        CancellationToken cancellationToken = default)
    {
        if (productItemEntryRequest.CostPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(productItemEntryRequest.CostPrice), "مبلغ بهای خرید نمی‌تواند منفی باشد.");

        if (productItemEntryRequest.CostPriceExchangeRate is <= 0)
            throw new ArgumentOutOfRangeException(nameof(productItemEntryRequest.CostPriceExchangeRate), "نرخ تبدیل باید بزرگتر از صفر باشد.");

        // Determine debit ledger based on product type
        LedgerAccountId debitLedgerId;
        if (product.ProductType == ProductType.MoltenGold)
        {
            var moltenInventory = await ledgerAccountRepository
                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.MoltenGoldInventory))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("حساب موجودی طلای آبشده یافت نشد.");
            debitLedgerId = moltenInventory.Id;
        }
        else
        {
            var inventoryLedger = await ledgerAccountRepository
                .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Inventory))
                .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("حساب موجودی کالا یافت نشد.");
            debitLedgerId = inventoryLedger.Id;
        }

        var openingEquity = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.OpeningBalanceEquity))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("حساب سرمایه افتتاحیه - تعدیلات یافت نشد.");

        var priceUnitId = new PriceUnitId(productItemEntryRequest.CostPriceUnitId);
        var description = TransactionDescriptionBuilder.ForManualProductEntry(product.Name);
        var groupId = Guid.NewGuid();
        var postingDate = inventoryStock.PostingDate;

        var amount = productItemEntryRequest.CostPrice;
        var exchangeRate = productItemEntryRequest.UnitPrice * (productItemEntryRequest.CostPriceExchangeRate ?? 1);
        var baseCurrencyAmount = amount * exchangeRate;

        var debit = Transaction.CreateForInventoryEntry(
            description,
            amount,
            baseCurrencyAmount,
            exchangeRate,
            groupId,
            TransactionType.Debit,
            debitLedgerId,
            priceUnitId,
            inventoryEntry.Id,
            inventoryStock.Id,
            postingDate);

        var credit = Transaction.CreateForInventoryEntry(
            description,
            amount,
            baseCurrencyAmount,
            exchangeRate,
            groupId,
            TransactionType.Credit,
            openingEquity.Id,
            priceUnitId,
            inventoryEntry.Id,
            inventoryStock.Id,
            postingDate);

        await repository.CreateRangeAsync([debit, credit], cancellationToken);
    }

    public async Task CreateForInventoryEntryAsync(
        InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        CreateCoinItemEntryRequest coinItemEntry,
        CancellationToken cancellationToken = default)
    {
        if (coinItemEntry.Quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(coinItemEntry.Quantity), "تعداد سکه باید بزرگتر از صفر باشد.");

        if (coinItemEntry.UnitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(coinItemEntry.UnitPrice), "قیمت واحد نمی‌تواند منفی باشد.");

        var coinId = new CoinId(coinItemEntry.CoinId);
        var coin = await coinRepository
            .Get(new CoinsByIdSpecification(coinId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"سکه {coinId.Value} یافت نشد.");

        var coinLedger = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(LedgerAccountTitleBuilder.ForCoinAccount(coin.Title)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"حساب دفتری سکه '{coin.Title}' یافت نشد.");

        var openingEquity = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.OpeningBalanceEquity))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("حساب سرمایه افتتاحیه - تعدیلات یافت نشد.");

        var basePriceUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("واحد ارزی پایه یافت نشد.");

        var description = TransactionDescriptionBuilder.ForManualCoinEntry(coin.Title);
        var groupId = Guid.NewGuid();
        var postingDate = inventoryStock.PostingDate;

        // Valuation in base currency: UnitPrice * Quantity
        var amount = coinItemEntry.UnitPrice * coinItemEntry.Quantity;
        var baseCurrencyAmount = amount; // Since UnitPrice is already in base currency

        var debit = Transaction.CreateForInventoryEntry(
            description,
            amount,
            baseCurrencyAmount,
            null,
            groupId,
            TransactionType.Debit,
            coinLedger.Id,
            basePriceUnit.Id,
            inventoryEntry.Id,
            inventoryStock.Id,
            postingDate);

        var credit = Transaction.CreateForInventoryEntry(
            description,
            amount,
            baseCurrencyAmount,
            null,
            groupId,
            TransactionType.Credit,
            openingEquity.Id,
            basePriceUnit.Id,
            inventoryEntry.Id,
            inventoryStock.Id,
            postingDate);

        await repository.CreateRangeAsync([debit, credit], cancellationToken);
    }

    public async Task CreateForInventoryEntryAsync(
        InventoryEntry inventoryEntry,
        InventoryStock inventoryStock,
        CreateCurrencyItemEntryRequest currencyItemEntry,
        CancellationToken cancellationToken = default)
    {
        if (currencyItemEntry.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(currencyItemEntry.Amount), "مقدار ارز باید بزرگتر از صفر باشد.");
        if (currencyItemEntry.UnitPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(currencyItemEntry.UnitPrice), "قیمت واحد ارز باید بزرگتر از صفر باشد.");

        var currencyId = new PriceUnitId(currencyItemEntry.CurrencyId);
        var currency = await priceUnitRepository
            .Get(new PriceUnitsByIdSpecification(currencyId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"واحد ارزی '{currencyId.Value}' یافت نشد.");

        // Ledger account is determined by financial account provided
        var financialAccount = await financialAccountRepository
            .Get(new FinancialAccountsByIdSpecification(new FinancialAccountId(currencyItemEntry.FinancialAccountId)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"حساب مالی {currencyItemEntry.FinancialAccountId} یافت نشد.");

        var currencyLedgerId = financialAccount.LedgerAccountId
                               ?? throw new NotFoundException($"برای حساب مالی {financialAccount.Id.Value} حساب دفتری متناظر تعریف نشده است.");

        var openingEquity = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.OpeningBalanceEquity))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("حساب سرمایه افتتاحیه - تعدیلات یافت نشد.");

        var description = TransactionDescriptionBuilder.ForManualCurrencyEntry(currency.Title);
        var groupId = Guid.NewGuid();
        var postingDate = inventoryStock.PostingDate;

        // Amount = currency units, ExchangeRate = unit price
        var amount = currencyItemEntry.Amount;
        var exchangeRate = currencyItemEntry.UnitPrice;

        var baseCurrencyAmount = amount * exchangeRate;

        var debit = Transaction.CreateForInventoryEntry(
            description,
            amount,
            baseCurrencyAmount,
            exchangeRate,
            groupId,
            TransactionType.Debit,
            currencyLedgerId,
            currencyId,
            inventoryEntry.Id,
            inventoryStock.Id,
            postingDate);

        var credit = Transaction.CreateForInventoryEntry(
            description,
            amount,
            baseCurrencyAmount,
            exchangeRate,
            groupId,
            TransactionType.Credit,
            openingEquity.Id,
            currencyId,
            inventoryEntry.Id,
            inventoryStock.Id,
            postingDate);

        await repository.CreateRangeAsync([debit, credit], cancellationToken);
    }

    public async Task AddWeightChangeTransactionAsync(ProductId id,
        decimal oldWeight,
        decimal newWeight,
        InventoryStockId? outStockId,
        InventoryStockId? inStockId,
        CancellationToken cancellationToken = default)
    {
        if (oldWeight == newWeight) return;

        var originStock = await inventoryStockRepository.Get(new InventoryStockOriginSpecification(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (originStock?.InventoryEntryId == null)
            throw new InvalidOperationException($"Origin stock not found for product {id.Value}.");

        var debitTransaction = await repository
            .Get(new TransactionsByInventoryEntryIdSpecification(originStock.InventoryEntryId.Value))
            .Where(t => t.InventoryStockId == originStock.Id && t.TransactionType == TransactionType.Debit)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (debitTransaction == null)
            throw new InvalidOperationException($"Financial record not found for product {id.Value}.");

        var pricePerGram = debitTransaction.ExchangeRate ?? 1;
        var priceUnitId = debitTransaction.PriceUnitId;
        var inventoryLedgerId = debitTransaction.LedgerAccountId;

        var equityLedger = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.OpeningBalanceEquity))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Opening Balance Equity ledger account not found.");

        var transactions = new List<Transaction>();
        var groupId = Guid.NewGuid();
        var now = DateTime.Now;
        var description = $"تصحیح وزن محصول (تغییر از {oldWeight.ToWeightFormat(GoldUnitType.Gram)} به {newWeight.ToWeightFormat(GoldUnitType.Gram)})";

        if (oldWeight > 0)
        {
            transactions.Add(Transaction.CreateForManualEntry(
                "برگشت: " + description,
                oldWeight,
                pricePerGram,
                groupId,
                TransactionType.Debit,
                equityLedger.Id,
                priceUnitId,
                null,
                outStockId,
                now));

            transactions.Add(Transaction.CreateForManualEntry(
                "برگشت: " + description,
                oldWeight,
                pricePerGram,
                groupId,
                TransactionType.Credit,
                inventoryLedgerId,
                priceUnitId,
                null,
                outStockId,
                now));
        }

        if (newWeight > 0)
        {
            var entryDate = transactions.Any() ? now.AddTicks(1) : now;

            transactions.Add(Transaction.CreateForManualEntry(
                "اصلاح: " + description,
                newWeight,
                pricePerGram,
                groupId,
                TransactionType.Debit,
                inventoryLedgerId,
                priceUnitId,
                default,
                inStockId,
                entryDate));

            transactions.Add(Transaction.CreateForManualEntry(
                "اصلاح: " + description,
                newWeight,
                pricePerGram,
                groupId,
                TransactionType.Credit,
                equityLedger.Id,
                priceUnitId,
                default,
                inStockId,
                entryDate));
        }

        if (transactions.Count > 0)
            await repository.CreateRangeAsync(transactions, cancellationToken);
    }

    public async Task CreateForInventoryExitAsync(
    InventoryExitId inventoryExitId,
    CreateInventoryExitRequest request,
    List<InventoryStock> inventoryStocks,
    CancellationToken cancellationToken)
    {
        if (inventoryStocks == null || !inventoryStocks.Any())
            throw new ArgumentException("No inventory stocks provided.", nameof(inventoryStocks));

        var exitReasonLedgerAccountTitle = request.ExitReason.GetLedgerAccount();

        var exitLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByTitleSpecification(exitReasonLedgerAccountTitle))
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Ledger account for exit reason '{request.ExitReason}' not found.");

        var basePriceUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Default price unit not found.");

        var transactions = new List<Transaction>();
        var tick = 1;

        foreach (var inventoryStock in inventoryStocks)
        {
            if (inventoryStock.ProductId.HasValue)
            {
                var product = await productRepository
                    .Get(new ProductsByIdSpecification(inventoryStock.ProductId.Value))
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException($"Product {inventoryStock.ProductId.Value.Value} not found.");

                var currentPrice = await priceService.GetAsync(product.GoldUnitType, basePriceUnit.Id.Value, false, cancellationToken);
                if (currentPrice == null || string.IsNullOrWhiteSpace(currentPrice.Value) || !decimal.TryParse(currentPrice.Value, out var pricePerGram))
                    throw new NotFoundException($"Current gram price for product '{product.Id.Value}' not found or invalid.");

                var priceUnit = await priceUnitRepository
                    .Get(new PriceUnitsByUnitTypeSpecification(currentPrice.UnitType!.Value))
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException($"Price unit for type '{currentPrice.UnitType.Value}' not found.");

                var amount = inventoryStock.ChangeAmount;
                var basePriceAmount = amount * pricePerGram;

                var groupId = Guid.NewGuid();
                var postingDate = inventoryStock.PostingDate;

                var description = TransactionDescriptionBuilder.ForInventoryExit(request.ExitReason, product);

                // حسابِ موجودی محصولات
                var productInventoryLedger = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.Inventory))
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException("Inventory ledger account not found.");

                var debit = Transaction.CreateForInventoryExit(
                    description,
                    amount,
                    basePriceAmount,
                    pricePerGram,
                    groupId,
                    TransactionType.Debit,
                    exitLedgerAccount.Id, 
                    priceUnit.Id,
                    inventoryExitId,
                    inventoryStock.Id,
                    postingDate.AddTicks(tick++)
                );

                var credit = Transaction.CreateForInventoryExit(
                    description,
                    amount,
                    basePriceAmount,
                    pricePerGram,
                    groupId,
                    TransactionType.Credit,
                    productInventoryLedger.Id, 
                    priceUnit.Id,
                    inventoryExitId,
                    inventoryStock.Id,
                    postingDate.AddTicks(tick++)
                );

                transactions.Add(debit);
                transactions.Add(credit);
            }
            else if (inventoryStock.CoinId.HasValue)
            {
                var coin = await coinRepository
                    .Get(new CoinsByIdSpecification(inventoryStock.CoinId.Value))
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException($"Coin {inventoryStock.CoinId.Value.Value} not found.");

                var coinInventoryLedger = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(LedgerAccountTitleBuilder.ForCoinAccount(coin.Title)))
                    .FirstOrDefaultAsync(cancellationToken)
                    ?? throw new NotFoundException($"Ledger account for coin '{coin.Title}' not found.");

                var coinCurrentPrice = await coinService.GetPriceAsync(coin.Id.Value, basePriceUnit.Id.Value, cancellationToken);
                if (coinCurrentPrice?.ExchangeRate == null)
                    throw new NotFoundException($"Current price for coin '{coin.Title}' not found.");

                var amount = inventoryStock.ChangeAmount;
                var exchangeRate = coinCurrentPrice.ExchangeRate.Value;
                var basePriceAmount = amount * exchangeRate;

                var groupId = Guid.NewGuid();
                var postingDate = inventoryStock.PostingDate;
                var description = TransactionDescriptionBuilder.ForInventoryExit(request.ExitReason, coin);

                var debit = Transaction.CreateForInventoryExit(
                    description,
                    amount,
                    basePriceAmount,
                    exchangeRate,
                    groupId,
                    TransactionType.Debit,
                    exitLedgerAccount.Id,
                    basePriceUnit.Id,
                    inventoryExitId,
                    inventoryStock.Id,
                    postingDate.AddTicks(tick++)
                );

                var credit = Transaction.CreateForInventoryExit(
                    description,
                    amount,
                    basePriceAmount,
                    exchangeRate,
                    groupId,
                    TransactionType.Credit,
                    coinInventoryLedger.Id,
                    basePriceUnit.Id,
                    inventoryExitId,
                    inventoryStock.Id,
                    postingDate.AddTicks(tick++)
                );

                transactions.Add(debit);
                transactions.Add(credit);
            }
            else
            {
                throw new InvalidOperationException("InventoryStock must be product or coin for InventoryExit.");
            }
        }

        if (transactions.Any()) 
            await repository.CreateRangeAsync(transactions, cancellationToken);
    }
}