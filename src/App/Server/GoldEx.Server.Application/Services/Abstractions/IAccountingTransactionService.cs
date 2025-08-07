using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IAccountingTransactionService
{
    Task CreateTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken);
    Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken);
}