using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
public class PriceRepository(GoldExDbContext context) : RepositoryBase<Price>(context), IPriceRepository
{
    public async Task<List<Price>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        var latestPrices = (await Query
                .Where(x => x.PriceHistories.Any())
                .ToListAsync(cancellationToken))
            .Select(p => Price.CreateFromSnapshot(p.Id, p.Title, p.PriceType, p.GetLatestPriceHistory()!));

        return latestPrices.ToList();
    }

    public Task<List<Price>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return Query.ToListAsync(cancellationToken);
    }
}