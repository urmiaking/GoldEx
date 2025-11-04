using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ITransactionRepository : IRepository<Transaction>,
    ICreateRepository<Transaction>,
    IUpdateRepository<Transaction>,
    IDeleteRepository<Transaction>
{
    Task RemoveByInvoiceIdAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default);
    Task RemoveByPaymentVoucherIdAsync(PaymentVoucherId paymentVoucherId, CancellationToken cancellationToken = default);
    Task RemoveByInvoicePaymentIdsAsync(List<InvoicePaymentId>? invoicePaymentIds, CancellationToken cancellationToken = default);

    Task<Dictionary<PriceUnit, decimal>> GetCustomerRemainingListAsync(CustomerId customerId, DateTime? untilDate = null, CancellationToken cancellationToken = default);
    Task<(decimal qty, decimal baseAmount, decimal avgRate)> GetLedgerPositionSummaryAsync(
        LedgerAccountId ledgerAccountId,
        CancellationToken cancellationToken = default);
}