using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceHistoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
public class PriceHistoryRepository(GoldExDbContext context) : RepositoryBase<PriceHistory>(context), IPriceHistoryRepository
{
    public async Task<int> CleanupAsync(CancellationToken cancellationToken = default)
    {
        var totalDeletedCount = 0; // Keep track of the total deleted count

        // 1. Get all PriceIds that have more than 10 history entries.
        var priceIdsToCleanup = await Query
            .GroupBy(ph => ph.PriceId)
            .Where(g => g.Count() > 10)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

        foreach (var priceId in priceIdsToCleanup)
        {
            // 2. Get the PriceHistories for the current PriceId, ordered by LastUpdate.
            var histories = await Query
                .Where(ph => ph.PriceId == priceId)
                .OrderBy(ph => ph.Id) // Important: Order by LastUpdate (EF.Property if string)
                .ToListAsync(cancellationToken);

            // 3. Calculate how many entries to remove.
            var numberOfEntriesToRemove = histories.Count - 10;

            if (numberOfEntriesToRemove > 0)
            {
                var entriesToRemove = histories.Take(numberOfEntriesToRemove).ToList();
                context.RemoveRange(entriesToRemove);
                totalDeletedCount += entriesToRemove.Count;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return totalDeletedCount;
    }
}