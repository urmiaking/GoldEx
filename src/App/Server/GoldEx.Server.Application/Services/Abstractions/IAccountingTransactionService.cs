using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IAccountingTransactionService
{
    Task SetTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken);
    Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken);
}