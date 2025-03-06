using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Shared.Infrastructure.Repositories;

public class PriceHistoryRepository<T>(IGoldExDbContextFactory factory) : RepositoryBase<T>(factory), IPriceHistoryRepository<T>
    where T : PriceHistoryBase
{
    public async Task<int> CleanupAsync(CancellationToken cancellationToken = default)
    {
        var totalDeletedCount = 0; // Keep track of the total deleted count

        await InitializeDbContextAsync();

        // 1. Get all PriceIds that have more than 10 history entries.
        var priceIdsToCleanup = await NonDeletedQuery
            .GroupBy(ph => ph.PriceId)
            .Where(g => g.Count() > 10)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

        foreach (var priceId in priceIdsToCleanup)
        {
            // 2. Get the PriceHistories for the current PriceId, ordered by LastUpdate.
            var histories = await NonDeletedQuery
                .Where(ph => ph.PriceId == priceId)
                .OrderBy(ph => ph.Id) // Important: Order by LastUpdate (EF.Property if string)
                .ToListAsync(cancellationToken);

            // 3. Calculate how many entries to remove.
            var numberOfEntriesToRemove = histories.Count - 10;

            if (numberOfEntriesToRemove > 0)
            {
                var entriesToRemove = histories.Take(numberOfEntriesToRemove).ToList();
                Context.RemoveRange(entriesToRemove);
                totalDeletedCount += entriesToRemove.Count;
            }
        }

        await Context.SaveChangesAsync(cancellationToken);

        return totalDeletedCount;
    }
}