using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
public class PriceRepository(GoldExDbContext context) : RepositoryBase<Price>(context), IPriceRepository
{
    public Task<List<Price>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        return Query
            .Include(p => p.PriceHistories!
                .OrderByDescending(ph => ph.Id)
                .Take(1))
            .ToListAsync(cancellationToken);
    }

    public Task<List<Price>> GetListAsync(CancellationToken cancellationToken = default)
    {
        return Query.ToListAsync(cancellationToken);
    }
}