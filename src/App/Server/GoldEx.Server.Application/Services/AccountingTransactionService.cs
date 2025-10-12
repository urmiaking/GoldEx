using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
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
    IServerLedgerAccountService ledgerAccountService) : IAccountingTransactionService
{
    public async Task SetTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        if (invoice is { TotalAmountWithDiscountsAndExtraCosts: 0, TotalPaidAmount: 0 }) return;

        var customer = await customerRepository.Get(new CustomersByIdSpecification(invoice.CustomerId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Customer {invoice.CustomerId.Value} not found.");

        var basePriceUnit = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Default price unit not found.");

        var transactions = new List<Transaction>();
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

                        transactions.Add(Transaction.CreateForInvoice(
                            TransactionDescriptionBuilder.ForSaleReceivable(invoice, customer),
                            invoice.TotalAmountWithDiscountsAndExtraCosts,
                            invoice.ExchangeRate,
                            invoiceGroupId,
                            TransactionType.Debit,
                            customerLedger.Id,
                            invoice.PriceUnitId,
                            invoice.Id));

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
                                invoice.Id));

                        if (invoice.TotalAmount > 0)
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForSaleRevenue(invoice),
                                invoice.TotalAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Credit,
                                salesRevenueLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id));

                        if (invoice.TotalExtraCostAmount > 0)
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForSaleExtraCharges(invoice),
                                invoice.TotalExtraCostAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Credit,
                                extraChargesLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id));

                        decimal totalCostOfGoods = 0;

                        foreach (var invoiceProductItem in invoice.ProductItems)
                        {

                            var purchaseInvoice = await invoiceRepository
                                .Get(new InvoicesByProductIdSpecification(invoiceProductItem.ProductId))
                                .FirstOrDefaultAsync(cancellationToken) 
                                                  ?? throw new NotFoundException($"Purchase invoice for product {invoiceProductItem.ProductId.Value} not found.");

                            totalCostOfGoods += purchaseInvoice.TotalAmount * (purchaseInvoice.ExchangeRate ?? 1);
                        }

                        var cogsAmount = totalCostOfGoods;
                        if (cogsAmount > 0)
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
                                invoice.Id));

                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForInventoryExit(invoice),
                                totalCostOfGoods,
                                null,
                                cogsGroupId,
                                TransactionType.Credit,
                                inventoryLedger.Id,
                                basePriceUnit.Id,
                                invoice.Id));
                        }

                        foreach (var instantProduct in invoice.ProductItems.Where(x => x.IsInstantProduct))
                        {
                            await CreateTransactionForManualEntryAsync(instantProduct.Product, null, null,
                                instantProduct.CostPrice!.Value, instantProduct.CostPriceExchangeRate, invoice.Id,
                                cancellationToken);
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
                                invoiceGroupId,
                                TransactionType.Debit,
                                inventoryLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id));

                        transactions.Add(Transaction.CreateForInvoice(
                            TransactionDescriptionBuilder.ForPurchasePayable(invoice, customer),
                            invoice.TotalAmountWithDiscountsAndExtraCosts,
                            invoice.ExchangeRate,
                            invoiceGroupId,
                            TransactionType.Credit,
                            supplierLedger.Id,
                            invoice.PriceUnitId,
                            invoice.Id));

                        if (invoice.TotalDiscountAmount > 0)
                            transactions.Add(Transaction.CreateForInvoice(
                                TransactionDescriptionBuilder.ForPurchaseDiscount(invoice, customer,
                                    invoice.Discounts.Select(x => x.Description)),
                                invoice.TotalDiscountAmount,
                                invoice.ExchangeRate,
                                invoiceGroupId,
                                TransactionType.Credit,
                                discountsLedger.Id,
                                invoice.PriceUnitId,
                                invoice.Id));

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (invoice.InvoicePayments is not null)
        {
            foreach (var payment in invoice.InvoicePayments)
            {
                var paymentGroupId = Guid.NewGuid();

                // فقط پرداخت‌های مستقیم (نه ووچر) تراکنش مالی جدید ایجاد می‌کنند
                if (payment.SourceFinancialAccountId.HasValue)
                {
                    var sourceFinancialAccount = await financialAccountRepository
                        .Get(new FinancialAccountsByIdSpecification(payment.SourceFinancialAccountId.Value))
                        .FirstOrDefaultAsync(cancellationToken) 
                                                 ?? throw new NotFoundException($"Financial account {payment.SourceFinancialAccountId.Value} not found.");

                    if (!sourceFinancialAccount.LedgerAccountId.HasValue)
                        throw new NotFoundException(
                            $"Financial account {sourceFinancialAccount.Id.Value} does not have a linked ledger account.");

                    var sourceLedgerAccount = await ledgerAccountRepository
                        .Get(new LedgerAccountsByIdSpecification(sourceFinancialAccount.LedgerAccountId.Value))
                        .FirstOrDefaultAsync(cancellationToken)
                                              ?? throw new NotFoundException($"Ledger account {sourceFinancialAccount.LedgerAccountId.Value} not found.");

                    if (invoice.InvoiceType == InvoiceType.Sell)
                    {
                        // --- منطق دریافت وجه از مشتری ---
                        var customerLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(customer.Id,
                            invoice.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                        // بدهکار کردن حساب بانک/صندوق شما (افزایش دارایی)
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForInvoicePaymentReceived(invoice, payment),
                            payment.Amount,
                            payment.ExchangeRate,
                            paymentGroupId,
                            TransactionType.Debit,
                            sourceLedgerAccount.Id,
                            payment.PriceUnitId,
                            payment.Id));

                        // بستانکار کردن حساب مشتری (کاهش طلب شما)
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForInvoicePaymentReceived(invoice, payment),
                            payment.Amount,
                            payment.ExchangeRate,
                            paymentGroupId,
                            TransactionType.Credit,
                            customerLedger.Id,
                            payment.PriceUnitId,
                            payment.Id));
                    }
                    else
                    {
                        // --- منطق پرداخت وجه به تامین‌کننده ---
                        var supplierLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(customer.Id,
                            payment.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        // بدهکار کردن حساب تامین‌کننده (کاهش بدهی شما)
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForInvoicePaymentMade(invoice, payment),
                            payment.Amount,
                            payment.ExchangeRate,
                            paymentGroupId,
                            TransactionType.Debit,
                            supplierLedger.Id,
                            payment.PriceUnitId,
                            payment.Id));

                        // بستانکار کردن حساب بانک/صندوق شما (کاهش دارایی)
                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForInvoicePaymentMade(invoice, payment),
                            payment.Amount,
                            payment.ExchangeRate,
                            paymentGroupId,
                            TransactionType.Credit,
                            sourceLedgerAccount.Id,
                            payment.PriceUnitId,
                            payment.Id));
                    }
                }
                else if (payment is
                {
                    PaymentVoucherId: not null, PaymentVoucher: not null
                }) // حالت ۲: اعمال پیش‌پرداخت (Voucher)
                {
                    if (invoice.InvoiceType == InvoiceType.Purchase)
                    {
                        var supplierLedger = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(customer.Id,
                            invoice.PriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                        var prepaymentLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.PrepaymentsToSuppliers))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Prepayments to Suppliers ledger account not found.");

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForPaymentVoucher(payment.PaymentVoucher, invoice),
                            payment.Amount,
                            payment.ExchangeRate,
                            paymentGroupId,
                            TransactionType.Debit,
                            supplierLedger.Id,
                            payment.PriceUnitId,
                            payment.Id));

                        transactions.Add(Transaction.CreateForInvoicePayment(
                            TransactionDescriptionBuilder.ForPaymentVoucher(payment.PaymentVoucher, invoice),
                            payment.Amount,
                            payment.ExchangeRate,
                            paymentGroupId,
                            TransactionType.Credit,
                            prepaymentLedger.Id,
                            payment.PriceUnitId,
                            payment.Id));
                    }
                }
            }
        }

        if (invoice.UsedProducts.Any())
            foreach (var usedProduct in invoice.UsedProducts)
                await CreateTransactionForUsedProductsAsync(usedProduct, cancellationToken);

        // 5. ذخیره تمام تراکنش‌های جدید
        await repository.CreateRangeAsync(transactions, cancellationToken);
    }

    public async Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher,
        CancellationToken cancellationToken = default)
    {
        await repository.RemoveByPaymentVoucherIdAsync(voucher.Id, cancellationToken);

        if (voucher.Amount == 0)
            return;

        var groupId = Guid.NewGuid();
        var transactions = new List<Transaction>();

        var sourceFinancialAccount = await financialAccountRepository
            .Get(new FinancialAccountsByIdSpecification(voucher.SourceFinancialAccountId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Financial account {voucher.SourceFinancialAccountId.Value} not found.");

        if (!sourceFinancialAccount.LedgerAccountId.HasValue)
            throw new NotFoundException(
                $"Financial account {sourceFinancialAccount.Id.Value} does not have a linked ledger account.");

        var creditLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByIdSpecification(sourceFinancialAccount.LedgerAccountId.Value))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Ledger account {sourceFinancialAccount.LedgerAccountId.Value} not found.");

        var voucherCustomer = await customerRepository.Get(new CustomersByIdSpecification(voucher.CustomerId))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Customer {voucher.CustomerId.Value} not found.");

        LedgerAccount debitLedgerAccount;
        string description;

        switch (voucher.VoucherType)
        {
            case PaymentVoucherType.PrepaymentToSupplier:
                {
                    // برای پیش‌پرداخت، حساب پرداختنی تامین‌کننده بدهکار می‌شود
                    debitLedgerAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(voucher.CustomerId,
                        voucher.VoucherPriceUnitId, LedgerAccountRole.Payable, cancellationToken);

                    description = TransactionDescriptionBuilder.ForPrepaymentToCustomer(voucher, voucherCustomer);
                    break;
                }

            case PaymentVoucherType.RefundToCustomer:
                {
                    // برای بازپرداخت، حساب دریافتنی مشتری بدهکار می‌شود
                    debitLedgerAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(voucher.CustomerId,
                        voucher.VoucherPriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

                    description = TransactionDescriptionBuilder.ForRefundToCustomer(voucher, voucherCustomer);
                    break;
                }

            case PaymentVoucherType.ServiceFeePayment:
                {
                    debitLedgerAccount = await ledgerAccountRepository
                        .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.ServiceExpenses)) // سرفصل جدید: "هزینه خدمات"
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Service Expenses account not found.");

                    description = TransactionDescriptionBuilder.ForServiceFeePayment(voucher, voucherCustomer);
                    break;
                }
            case PaymentVoucherType.PartnerLoan:
                {
                    debitLedgerAccount = await ledgerAccountRepository
                        .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.LoansToOthers))
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("LoansToOthers account not found.");

                    description = TransactionDescriptionBuilder.ForPartnerLoan(voucher, voucherCustomer);
                    break;
                }
            case PaymentVoucherType.OwnerDraw:
                {
                    // حساب برداشت مالک بدهکار می‌شود
                    debitLedgerAccount = await ledgerAccountRepository
                        .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.OwnerDraw)) // سرفصل جدید: "برداشت مالک"
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Owner Draw account not found.");

                    description = TransactionDescriptionBuilder.ForOwnerDraw(voucher, voucherCustomer);
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException(nameof(voucher.VoucherType));
        }

        // بدهکار کردن حساب مقصد پرداخت (حساب پرداختنی مشتری یا هزینه خدمات یا برداشت مالک)
        transactions.Add(Transaction.CreateForPaymentVoucher(
            description,
            voucher.Amount,
            voucher.ExchangeRate,
            groupId,
            TransactionType.Debit,
            debitLedgerAccount.Id,
            voucher.VoucherPriceUnitId,
            voucher.Id));

        // بستانکار کردن حساب بانک/صندوق شما
        transactions.Add(Transaction.CreateForPaymentVoucher(
            description,
            voucher.Amount,
            voucher.ExchangeRate,
            groupId,
            TransactionType.Credit,
            creditLedgerAccount.Id,
            voucher.VoucherPriceUnitId,
            voucher.Id));

        await repository.CreateRangeAsync(transactions, cancellationToken);
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
                meltingBatchId: meltingBatchId
            );

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
                meltingBatchId: meltingBatchId
            );

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
            meltingBatchId: meltingBatch.Id
        );

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
            meltingBatchId: meltingBatch.Id
        );

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
                meltingBatchId: meltingBatch.Id
            );

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
                meltingBatchId: meltingBatch.Id
            );

            transactions.Add(feeDebit);
            transactions.Add(feeCredit);
        }

        // ذخیره تراکنش‌ها
        await repository.CreateRangeAsync(transactions, cancellationToken);
    }

    private async Task CreateTransactionForManualEntryAsync(Product? product, Coin? coin, PriceUnit? currency,
        decimal costPrice, decimal? costPriceExchangeRate, InvoiceId triggeringInvoiceId,
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
            basePriceUnit.Id,
            triggeringInvoiceId));

        transactions.Add(Transaction.CreateForManualEntry(
            description,
            costPrice,
            costPriceExchangeRate,
            groupId,
            TransactionType.Credit,
            openingBalanceLedgerAccount.Id,
            basePriceUnit.Id,
            triggeringInvoiceId));

        await repository.CreateRangeAsync(transactions, cancellationToken);
    }

    private async Task CreateTransactionForUsedProductsAsync(InvoiceUsedProduct usedProduct, CancellationToken cancellationToken = default)
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

        var debitLedgerAccountId = inventoryLedgerAccount.Id;

        var customerLedgerAccount = await ledgerAccountService.GetOrCreateCustomerSubLedgerAsync(customer.Id,
            usedProduct.Invoice.PriceUnitId, LedgerAccountRole.Receivable, cancellationToken);

        // --- Step 3: Create the Debit and Credit Transactions ---
        var transactions = new List<Transaction>();
        var groupId = Guid.NewGuid();
        var description = TransactionDescriptionBuilder.ForUsedProductPurchase(usedProduct, customer);

        transactions.Add(Transaction.CreateForInvoice(
            description,
            usedProduct.ItemFinalAmount,
            usedProduct.Invoice.ExchangeRate,
            groupId,
            TransactionType.Debit,
            debitLedgerAccountId,
            usedProduct.Invoice.PriceUnitId,
            usedProduct.InvoiceId
        ));

        transactions.Add(Transaction.CreateForInvoice(
            description,
            usedProduct.ItemFinalAmount,
            usedProduct.Invoice.ExchangeRate,
            groupId,
            TransactionType.Credit,
            customerLedgerAccount.Id,
            usedProduct.Invoice.PriceUnitId,
            usedProduct.InvoiceId
        ));

        await repository.CreateRangeAsync(transactions, cancellationToken);
    }
}