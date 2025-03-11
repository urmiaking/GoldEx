using System.Linq.Dynamic.Core;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Shared.Infrastructure.Repositories;

public class SettingsRepository<T>(IGoldExDbContextFactory contextFactory) : RepositoryBase<T>(contextFactory), ISettingsRepository<T> where T : SettingsBase
{
    public async Task<T?> GetAsync(SettingsId id, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<T?> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<T?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        var settings = await NonDeletedQuery.FirstOrDefaultAsync(cancellationToken);

        if (settings != null && settings.LastModifiedDate >= checkpointDate)
            return settings;

        return null;
    }
}