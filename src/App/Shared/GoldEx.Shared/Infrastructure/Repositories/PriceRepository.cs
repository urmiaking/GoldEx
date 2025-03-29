using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Entities;
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

        return await NonDeletedQuery.ToListAsync(cancellationToken);
    }

    public async Task<TPrice?> GetAsync(PriceId id, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();
        
        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<TPrice>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        List<TPrice> result = [];

        if (typeof(TPriceHistory).IsAssignableTo(typeof(ISyncableEntity)))
        {
            result = await NonDeletedQuery
                .Where(x => x.PriceHistory.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.PriceHistory.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        return result;
    }

    public async Task<TPrice?> GetGram18PriceAsync(CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Title.Contains("18 عیار"), cancellationToken);
    }

    public async Task<TPrice?> GetUsDollarPriceAsync(CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        var result = await NonDeletedQuery.FirstOrDefaultAsync(x => x.Title.Equals("دلار"), cancellationToken);

        return result;
    }
}