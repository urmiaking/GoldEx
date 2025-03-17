using System.Linq.Dynamic.Core;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Shared.Infrastructure.Repositories;

public class ProductCategoryRepository<T>(IGoldExDbContextFactory factory) : RepositoryBase<T>(factory), IProductCategoryRepository<T> where T : ProductCategoryBase
{
    public async Task<T?> GetAsync(ProductCategoryId id, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<T?> GetAsync(string title, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Title == title, cancellationToken);
    }

    public async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.ToListAsync(cancellationToken);
    }

    public async Task<List<T>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        // ClientSide pending items
        if (typeof(T).IsAssignableTo(typeof(ITrackableEntity)))
        {
            return await AllQuery
                .Where($"{nameof(ITrackableEntity.Status)}<>{(int)ModifyStatus.Synced}")
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        // Serverside pending items
        if (typeof(T).IsAssignableTo(typeof(ISoftDeleteEntity)))
        {
            return await AllQuery
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        return [];
    }
}