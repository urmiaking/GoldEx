using GoldEx.Client.Offline.Domain.CheckpointAggregate;
using GoldEx.Client.Offline.Infrastructure.Repositories.Abstractions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.Infrastructure;
using GoldEx.Shared.Infrastructure.Extensions;
using GoldEx.Shared.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Client.Offline.Infrastructure.Repositories;

[ScopedService]
public class CheckPointRepository(IGoldExDbContextFactory contextFactory) : RepositoryBase<Checkpoint>(contextFactory), ICheckPointRepository
{
    public async Task<Checkpoint?> GetLastCheckPointAsync(string entityName, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery
            .SetTracking(false)
            .Where(c => c.EntityName == entityName)
            .OrderByDescending(x => x.DateTime)
            .FirstOrDefaultAsync(cancellationToken);
    }
}