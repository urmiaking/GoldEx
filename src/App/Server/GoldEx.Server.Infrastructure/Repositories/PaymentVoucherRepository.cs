using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class PaymentVoucherRepository(GoldExDbContext dbContext) : RepositoryBase<PaymentVoucher>(dbContext), IPaymentVoucherRepository
{
    public async Task<long> GetLastNumberAsync(CancellationToken cancellationToken = default)
    {
        var voucherNumber = await Query
            .OrderByDescending(x => x.VoucherNumber)
            .Select(x => x.VoucherNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return voucherNumber;
    }
}