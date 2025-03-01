using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Shared.Infrastructure.Repositories;

public class PriceRepository<TPrice, TPriceHistory>(IGoldExDbContextFactory factory) : RepositoryBase<TPrice>(factory), IPriceRepository<TPrice, TPriceHistory>
    where TPrice : PriceBase<TPriceHistory>
    where TPriceHistory : PriceHistoryBase
{
    public async Task<List<TPrice>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await Query
            .Include(p => p.PriceHistories!
                .OrderByDescending(ph => ph.Id)
                .Take(1))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TPrice>> GetListAsync(CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();
        return await Query.ToListAsync(cancellationToken);
    }
}