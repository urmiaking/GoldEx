using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IAccountingTransactionService
{
    Task SetTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task CreateTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);
    Task ClearTransactionsForPaymentVoucherAsync(PaymentVoucher voucher, CancellationToken cancellationToken = default);
    Task ClearTransactionsForInvoiceAsync(Invoice invoice, CancellationToken cancellationToken = default);
    Task CreateTransactionForManualEntryAsync(Product? product, Coin? coin, PriceUnit? currency, 
        decimal costPrice, decimal? costPriceExchangeRate, InvoiceId triggeringInvoiceId,
        CancellationToken cancellationToken = default);
}