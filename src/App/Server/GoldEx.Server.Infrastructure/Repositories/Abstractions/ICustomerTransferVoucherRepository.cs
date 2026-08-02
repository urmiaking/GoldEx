using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ICustomerTransferVoucherRepository : IRepository<CustomerTransferVoucher>,
    ICreateRepository<CustomerTransferVoucher>,
    IUpdateRepository<CustomerTransferVoucher>,
    IDeleteRepository<CustomerTransferVoucher>
{
    Task<long> GetLastNumberAsync(CancellationToken cancellationToken = default);
}
