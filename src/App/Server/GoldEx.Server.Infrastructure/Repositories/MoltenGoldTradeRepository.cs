using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.MoltenGoldTradeAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class MoltenGoldTradeRepository(GoldExDbContext context) :
    RepositoryBase<MoltenGoldTrade>(context), IMoltenGoldTradeRepository
{
    public IQueryable<MoltenGoldTradeEvent> Events => context.Set<MoltenGoldTradeEvent>();

    public async Task LockAsync(CancellationToken cancellationToken)
    {
        if (!context.Database.IsSqlServer())
            return;
        var resource = $"goldex-molten-desk:{context.CurrentStoreId.Value}";
        await context.Database.ExecuteSqlInterpolatedAsync($"""
            DECLARE @result int;
            EXEC @result = sp_getapplock @Resource = {resource}, @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
            IF @result < 0 THROW 50001, 'Could not lock molten gold desk.', 1;
            """, cancellationToken);
    }

    public async Task AddEventAsync(MoltenGoldTradeEvent entry, CancellationToken cancellationToken)
    {
        context.Add(entry);
        await SaveAsync(cancellationToken);
    }
}
