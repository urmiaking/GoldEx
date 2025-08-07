using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class AccountingTransactionService(
    ITransactionRepository repository,
    IInvoiceRepository invoiceRepository,
    IPriceUnitRepository priceUnitRepository,
    IFinancialAccountRepository financialAccountRepository,
    ILedgerAccountRepository ledgerAccountRepository) : IAccountingTransactionService
{
    public async Task CreateTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        await repository.RemoveByInvoiceIdAsync(invoice.Id, cancellationToken);
        await repository.RemoveByInvoicePaymentIdsAsync(invoice.InvoicePayments?.Select(x => x.Id).ToList(), cancellationToken);

        if (invoice is { TotalAmountWithDiscountsAndExtraCosts: 0, TotalPaidAmount: 0 }) return;

        if (invoice.Customer is null)
            throw new ArgumentException("Invoice must have a customer associated with it.", nameof(invoice));

        var basePriceUnit = await priceUnitRepository.Get(new PriceUnitsDefaultSpecification())
                                .FirstOrDefaultAsync(cancellationToken) ??
                            throw new NotFoundException("Default price unit not found.");

        var transactions = new List<Transaction>();
        if (invoice.TotalAmountWithDiscountsAndExtraCosts != 0)
        {
            var invoiceGroupId = Guid.NewGuid();
            switch (invoice.InvoiceType)
            {
                case InvoiceType.Sell:
                {
                    var customerLedger = await ledgerAccountRepository
                                             .Get(new LedgerAccountsByCustomerAndParentTitleSpecification(
                                                 invoice.CustomerId,
                                                 SystemLedgerAccounts.AccountsReceivable))
                                             .FirstOrDefaultAsync(cancellationToken) ??
                                         throw new NotFoundException(
                                             $"Customer {invoice.CustomerId.Value} ledger account not found.");
                    var salesRevenueLedger = await ledgerAccountRepository
                                                 .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts
                                                     .SalesRevenue))
                                                 .FirstOrDefaultAsync(cancellationToken) ??
                                             throw new NotFoundException("Sales Revenue ledger account not found.");
                    var discountsLedger = await ledgerAccountRepository
                                              .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts
                                                  .SalesDiscounts))
                                              .FirstOrDefaultAsync(cancellationToken) ??
                                          throw new NotFoundException("Sales Discounts ledger account not found.");
                    var extraChargesLedger = await ledgerAccountRepository
                                                 .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts
                                                     .AdditionalChargesRevenue))
                                                 .FirstOrDefaultAsync(cancellationToken) ??
                                             throw new NotFoundException(
                                                 "Additional Charges Revenue ledger account not found.");

                    transactions.Add(Transaction.CreateForInvoice(
                        TransactionDescriptionBuilder.ForSaleReceivable(invoice, invoice.Customer),
                        invoice.TotalAmountWithDiscountsAndExtraCosts,
                        invoice.ExchangeRate,
                        invoiceGroupId,
                        TransactionType.Debit,
                        customerLedger.Id,
                        invoice.PriceUnitId,
                        invoice.Id));

                    if (invoice.TotalDiscountAmount > 0)
                        transactions.Add(Transaction.CreateForInvoice(
                            TransactionDescriptionBuilder.ForSaleDiscount(invoice, invoice.Discounts.Select(x => x.Description)),
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

                    foreach (var invoiceItem in invoice.Items)
                    {
                        if (!invoiceItem.SellProductId.HasValue)
                            continue;

                        var purchaseInvoice = await invoiceRepository
                                                  .Get(new InvoicesByProductIdSpecification(invoiceItem.SellProductId
                                                      .Value))
                                                  .FirstOrDefaultAsync(cancellationToken) ??
                                              throw new NotFoundException(
                                                  $"Purchase invoice for product {invoiceItem.SellProductId.Value} not found.");

                        totalCostOfGoods += purchaseInvoice.TotalAmount * (purchaseInvoice.ExchangeRate ?? 1);
                    }

                    var cogsAmount = totalCostOfGoods;
                    if (cogsAmount > 0)
                    {
                        var cogsGroupId = Guid.NewGuid();
                        var cogsLedger = await ledgerAccountRepository
                                             .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts
                                                 .CostOfGoodsSold))
                                             .FirstOrDefaultAsync(cancellationToken) ??
                                         throw new NotFoundException("Cost of Goods Sold ledger account not found.");
                        var inventoryLedger = await ledgerAccountRepository
                                                  .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts
                                                      .Inventory))
                                                  .FirstOrDefaultAsync(cancellationToken) ??
                                              throw new NotFoundException("Inventory ledger account not found.");

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

                    break;
                }

                case InvoiceType.Purchase:
                {
                    var supplierLedger = await ledgerAccountRepository
                                             .Get(new LedgerAccountsByCustomerAndParentTitleSpecification(
                                                 invoice.CustomerId,
                                                 SystemLedgerAccounts.AccountsPayable))
                                             .FirstOrDefaultAsync(cancellationToken) ??
                                         throw new NotFoundException(
                                             $"Supplier {invoice.CustomerId.Value} ledger account not found.");

                    var inventoryLedger = await ledgerAccountRepository
                                              .Get(new LedgerAccountsByTitleSpecification(
                                                  SystemLedgerAccounts.Inventory))
                                              .FirstOrDefaultAsync(cancellationToken) ??
                                          throw new NotFoundException("Inventory ledger account not found.");

                    var discountsLedger = await ledgerAccountRepository
                                              .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts
                                                  .PurchaseDiscounts))
                                              .FirstOrDefaultAsync(cancellationToken) ??
                                          throw new NotFoundException("Purchase Discounts ledger account not found.");

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
                        TransactionDescriptionBuilder.ForPurchasePayable(invoice, invoice.Customer),
                        invoice.TotalAmountWithDiscountsAndExtraCosts,
                        invoice.ExchangeRate,
                        invoiceGroupId,
                        TransactionType.Credit,
                        supplierLedger.Id,
                        invoice.PriceUnitId,
                        invoice.Id));

                    if (invoice.TotalDiscountAmount > 0)
                        transactions.Add(Transaction.CreateForInvoice(
                            TransactionDescriptionBuilder.ForPurchaseDiscount(invoice, invoice.Customer, invoice.Discounts.Select(x => x.Description)),
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
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Financial account {payment.SourceFinancialAccountId.Value} not found.");

                    if (!sourceFinancialAccount.LedgerAccountId.HasValue)
                        throw new NotFoundException($"Financial account {sourceFinancialAccount.Id.Value} does not have a linked ledger account.");

                    var sourceLedgerAccount = await ledgerAccountRepository
                        .Get(new LedgerAccountsByIdSpecification(sourceFinancialAccount.LedgerAccountId.Value))
                        .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Ledger account {sourceFinancialAccount.LedgerAccountId.Value} not found.");

                    if (invoice.InvoiceType == InvoiceType.Sell)
                    {
                        // --- منطق دریافت وجه از مشتری ---
                        var customerLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByCustomerAndParentTitleSpecification(invoice.CustomerId, SystemLedgerAccounts.AccountsReceivable))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Customer {invoice.CustomerId.Value} ledger account not found.");

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
                        var supplierLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByCustomerAndParentTitleSpecification(invoice.CustomerId, SystemLedgerAccounts.AccountsPayable))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Customer {invoice.CustomerId.Value} ledger account not found.");

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
                else if (payment is { PaymentVoucherId: not null, PaymentVoucher: not null }) // حالت ۲: اعمال پیش‌پرداخت (Voucher)
                {
                    if (invoice.InvoiceType == InvoiceType.Purchase)
                    {
                        var supplierLedger = await ledgerAccountRepository
                            .Get(new LedgerAccountsByCustomerAndParentTitleSpecification(invoice.CustomerId, SystemLedgerAccounts.AccountsPayable))
                            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Customer {invoice.CustomerId.Value} ledger account not found.");
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

        // 5. ذخیره تمام تراکنش‌های جدید
        await repository.CreateRangeAsync(transactions, cancellationToken);
    }

    public async Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken)
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
            throw new NotFoundException($"Financial account {sourceFinancialAccount.Id.Value} does not have a linked ledger account.");

        var sourceLedgerAccount = await ledgerAccountRepository
            .Get(new LedgerAccountsByIdSpecification(sourceFinancialAccount.LedgerAccountId.Value))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException($"Ledger account {sourceFinancialAccount.LedgerAccountId.Value} not found.");

        if (voucher.DestinationFinancialAccount is null)
            throw new ArgumentException("Payment voucher must have a destination financial account associated with it.", nameof(voucher));

        if (voucher.DestinationFinancialAccount.Customer is null)
            throw new ArgumentException("Payment voucher must have a customer associated with its destination financial account.", nameof(voucher));

        var destinationLedgerAccount = voucher.VoucherType switch
        {
            PaymentVoucherType.PrepaymentToSupplier =>
                await ledgerAccountRepository
                    .Get(new LedgerAccountsByCustomerAndParentTitleSpecification(
                        voucher.DestinationFinancialAccount.Customer.Id, SystemLedgerAccounts.AccountsPayable))
                    .FirstOrDefaultAsync(cancellationToken) ??
                throw new NotFoundException("Supplier's payable account not found."),
            PaymentVoucherType.RefundToCustomer =>
                await ledgerAccountRepository
                    .Get(new LedgerAccountsByCustomerAndParentTitleSpecification(
                        voucher.DestinationFinancialAccount.Customer.Id, SystemLedgerAccounts.AccountsReceivable))
                    .FirstOrDefaultAsync(cancellationToken) ??
                throw new NotFoundException("Customer's receivable account not found."),
            _ => throw new ArgumentOutOfRangeException(nameof(voucher.VoucherType))
        };

        // بدهکار کردن حساب پیش‌پرداخت (یک دارایی/طلب برای شما ایجاد می‌شود)
        transactions.Add(Transaction.CreateForPaymentVoucher(
            TransactionDescriptionBuilder.ForPrepaymentAsset(voucher, voucher.DestinationFinancialAccount.Customer),
            voucher.Amount,
            voucher.ExchangeRate,
            groupId,
            TransactionType.Debit,
            destinationLedgerAccount.Id,
            voucher.VoucherPriceUnitId,
            voucher.Id));

        // بستانکار کردن حساب بانک/صندوق شما (یک دارایی از شما کم می‌شود)
        transactions.Add(Transaction.CreateForPaymentVoucher(
            TransactionDescriptionBuilder.ForPrepaymentCashExit(voucher, voucher.DestinationFinancialAccount),
            voucher.Amount,
            voucher.ExchangeRate,
            groupId,
            TransactionType.Credit,
            sourceLedgerAccount.Id,
            voucher.VoucherPriceUnitId,
            voucher.Id));

        await repository.CreateRangeAsync(transactions, cancellationToken);
    }
}