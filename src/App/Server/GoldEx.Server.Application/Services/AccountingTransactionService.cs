using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.MeltingBatches;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
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
    ILedgerAccountRepository ledgerAccountRepository,
    IServerLedgerAccountService ledgerAccountService)
    : IAccountingTransactionService
{
    private static DateTime ComposePostingDate(DateOnly businessDate, DateTime createdAt, long tickOffset = 0)
        => businessDate.ToDateTime(TimeOnly.FromTimeSpan(createdAt.TimeOfDay)).AddTicks(tickOffset);

    private readonly record struct TransactionSignatureFull(
        TransactionType Type,
        LedgerAccountId LedgerId,
        PriceUnitId PriceUnitId,
        decimal Amount,
        decimal? ExchangeRate,
        DateTime PostingDate,
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
        new(t.TransactionType, t.LedgerAccountId, t.PriceUnitId, t.Amount, t.ExchangeRate, t.PostingDate,
            t.InvoiceId, t.InvoicePaymentId, t.PaymentVoucherId, t.MeltingBatchId);

    private static TransactionSignatureMatch SigMatch(Transaction t) =>
        new(t.TransactionType, t.LedgerAccountId, t.PriceUnitId, t.Amount, t.ExchangeRate,
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
                        var customerLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(customer.Id,
                            invoice.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

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
                            var manual = await CreateTransactionForManualEntryAsync(instantProduct.Product, null, null,
                                instantProduct.CostPrice!.Value, instantProduct.CostPriceExchangeRate, instantProduct.CostPriceUnitId, invoice.Id, NextLine(),
                                cancellationToken);
                            transactions.AddRange(manual);
                        }

                        transactions.Add(Transaction.CreateForInvoice(
                            TransactionDescriptionBuilder.ForSaleReceivable(invoice, customer),
                            invoice.TotalAmountWithDiscountsAndExtraCosts,
                            invoice.ExchangeRate,
                            invoiceGroupId,
                            TransactionType.Debit,
                            customerLedger.Id,
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

                        if (invoice.TotalAmount > 0)
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForSaleRevenue(invoice),
                                invoice.TotalAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Credit,
                                salesRevenueLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));

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

                        decimal totalCostOfGoods = 0;

                        foreach (var saleItem in invoice.ProductItems)
                        {
                            var purchaseInvoice = await invoiceRepository
                                .Get(new InvoicesByProductIdSpecification(saleItem.ProductId))
                                .OrderByDescending(x => x.InvoiceDate)
                                .FirstOrDefaultAsync(cancellationToken)
                                ?? throw new NotFoundException($"Purchase invoice for product {saleItem.ProductId.Value} not found.");

                            var purchaseItem = purchaseInvoice.ProductItems
                                .FirstOrDefault(i => i.ProductId == saleItem.ProductId)
                                ?? throw new NotFoundException($"Purchase invoice item for product {saleItem.ProductId.Value} not found.");

                            var purchaseItemBaseAmount = purchaseItem.ItemFinalAmount * (purchaseInvoice.ExchangeRate ?? 1);

                            var unitCost = purchaseItemBaseAmount / purchaseItem.TotalWeight;
                            var cogsForThisItem = unitCost * saleItem.TotalWeight;

                            totalCostOfGoods += cogsForThisItem;
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
                                totalCostOfGoods,
                                null,
                                cogsGroupId,
                                TransactionType.Debit,
                                cogsLedger.Id,
                                basePriceUnit.Id,
                                invoice.Id, NextLine()));

                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForInventoryExit(invoice),
                                totalCostOfGoods,
                                null,
                                cogsGroupId,
                                TransactionType.Credit,
                                inventoryLedger.Id,
                                basePriceUnit.Id,
                                invoice.Id, NextLine()));
                        }

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

                        var totalInventoryValue = invoice.TotalAmount + invoice.TotalExtraCostAmount;

                        if (totalInventoryValue > 0)
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForPurchaseInventoryEntry(invoice),
                                totalInventoryValue,
                                invoice.ExchangeRate,
                                Guid.NewGuid(),
                                TransactionType.Debit,
                                inventoryLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));

                        transactions.Add(Transaction.CreateForInvoice(
                            TransactionDescriptionBuilder.ForPurchasePayable(invoice, customer),
                            invoice.TotalAmountWithDiscountsAndExtraCosts,
                            invoice.ExchangeRate,
                            Guid.NewGuid(),
                            TransactionType.Credit,
                            supplierLedger.Id,
                            invoice.PriceUnitId,
                            invoice.Id, NextLine()));

                        if (invoice.TotalDiscountAmount > 0)
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForPurchaseDiscount(invoice, customer,
                                    invoice.Discounts.Select(x => x.Description)),
                                invoice.TotalDiscountAmount,
                                invoice.ExchangeRate,
                                Guid.NewGuid(),
                                TransactionType.Credit,
                                discountsLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id, NextLine()));

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
                DateTime NextPayLine() => payment.PaymentDate.AddTicks(postingTickOffset + payLine++);

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

                    if (invoice.InvoiceType == InvoiceType.Sell)
                    {
                        var customerLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            customer.Id, payment.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForInvoicePaymentReceived(invoice, payment),
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit, sourceLedgerAccount.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForInvoicePaymentReceived(invoice, payment),
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit, customerLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));
                    }
                    else
                    {
                        var supplierLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            customer.Id, payment.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForInvoicePaymentMade(invoice, payment),
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit, supplierLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForInvoicePaymentMade(invoice, payment),
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit, sourceLedgerAccount.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));
                    }
                }
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

                    if (invoice.InvoiceType == InvoiceType.Sell)
                    {
                        var customerLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            customer.Id, payment.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                        string description = payment.PaymentType switch
                        {
                            PaymentType.MoltenGoldInventory => TransactionDescriptionBuilder
                                .ForMoltenGoldPaymentReceived(invoice, payment),
                            PaymentType.UsedGoldInventory => TransactionDescriptionBuilder
                                .ForUsedGoldPaymentReceived(invoice, payment),
                            _ => throw new InvalidOperationException("Invalid gold payment type for Sell invoice.")
                        };

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit, goldLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit, customerLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));
                    }
                    else
                    {
                        var supplierLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            customer.Id, payment.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        var description = payment.PaymentType switch
                        {
                            PaymentType.MoltenGoldInventory => TransactionDescriptionBuilder
                                .ForMoltenGoldPaymentMade(invoice, payment),
                            PaymentType.UsedGoldInventory => TransactionDescriptionBuilder.ForUsedGoldPaymentMade(
                                invoice, payment),
                            _ => throw new InvalidOperationException(
                                "Invalid gold payment type for Purchase invoice.")
                        };

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Debit, supplierLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            description,
                            payment.FinalAmount, payment.ExchangeRate, paymentGroupId,
                            TransactionType.Credit, goldLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));
                    }
                }
                else if (payment.PaymentType == PaymentType.CustomerTransfer)
                {
                    if (payment.LedgerAccount is null)
                        throw new NotFoundException("Ledger account is required for CustomerTransfer payments.");

                    if (!payment.LedgerAccount.CustomerId.HasValue)
                        throw new InvalidOperationException("Endorser customer is required for CustomerTransfer payments.");

                    var endorserId = payment.LedgerAccount.CustomerId.Value;

                    if (invoice.InvoiceType == InvoiceType.Sell)
                    {
                        var endorserLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            endorserId, payment.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                        var desc = TransactionDescriptionBuilder.ForInvoicePaymentReceivedByEndorser(
                            invoice, payment, endorserLedger.Customer!.FullName);

                        var customerReceivableAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            invoice.CustomerId, payment.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            desc, payment.FinalAmount, payment.ExchangeRate, Guid.NewGuid(),
                            TransactionType.Debit, customerReceivableAccount.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            desc, payment.FinalAmount, payment.ExchangeRate, Guid.NewGuid(),
                            TransactionType.Credit, endorserLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));
                    }
                    else
                    {
                        var endorserLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            endorserId, payment.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        var desc = TransactionDescriptionBuilder.ForInvoicePaymentMadeByEndorser(
                            invoice, payment, endorserLedger.Customer!.FullName);

                        var customerPayableAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            invoice.CustomerId, payment.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            desc, payment.FinalAmount, payment.ExchangeRate, Guid.NewGuid(),
                            TransactionType.Debit, endorserLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            desc, payment.FinalAmount, payment.ExchangeRate, Guid.NewGuid(),
                            TransactionType.Credit, customerPayableAccount.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));
                    }
                }
                else if (payment is { PaymentVoucherId: not null, PaymentVoucher: not null })
                {
                    if (invoice.InvoiceType == InvoiceType.Purchase)
                    {
                        var supplierLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(
                            customer.Id, invoice.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        var prepaymentLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.PrepaymentsToSuppliers))
                            .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new NotFoundException("Prepayments to Suppliers ledger account not found.");

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForPaymentVoucher(payment.PaymentVoucher, invoice),
                            payment.Amount, payment.ExchangeRate, Guid.NewGuid(),
                            TransactionType.Debit, supplierLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForPaymentVoucher(payment.PaymentVoucher, invoice),
                            payment.Amount, payment.ExchangeRate, Guid.NewGuid(),
                            TransactionType.Credit, prepaymentLedger.Id, payment.PriceUnitId, invoice.Id, payment.Id, NextPayLine()));
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
        var activeFullSet = active.Select(SigFull).ToHashSet();
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
                amount: amount,
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
                amount: amount,
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
            ? TransactionDescriptionBuilder.ForManualProductEntry(product.Name,
                product.Barcode)
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

        var postingDate = ComposePostingDate(usedProduct.Invoice.InvoiceDate, usedProduct.Invoice.CreatedAt, ticks);

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

        return transactions;
    }
}