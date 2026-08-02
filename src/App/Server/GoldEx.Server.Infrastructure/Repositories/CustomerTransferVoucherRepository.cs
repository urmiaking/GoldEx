using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class CustomerTransferVoucherRepository(GoldExDbContext dbContext) : RepositoryBase<CustomerTransferVoucher>(dbContext), ICustomerTransferVoucherRepository
{
    public async Task<long> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var voucherNumber = await Query
            .AsNoTracking()
            .OrderByDescending(x => x.VoucherNumber)
            .Select(x => x.VoucherNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return voucherNumber;
    }
}
