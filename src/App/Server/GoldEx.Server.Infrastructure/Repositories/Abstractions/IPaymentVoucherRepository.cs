using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PaymentVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IPaymentVoucherRepository : IRepository<PaymentVoucher>,
    ICreateRepository<PaymentVoucher>,
    IUpdateRepository<PaymentVoucher>,
    IDeleteRepository<PaymentVoucher>
{
    Task<long> GetLastNumberAsync(CancellationToken cancellationToken = default);
}