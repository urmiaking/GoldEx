using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Extensions;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace GoldEx.Shared.Infrastructure.Repositories;

public class ProductRepository<T>(IGoldExDbContextFactory factory) : RepositoryBase<T>(factory), IProductRepository<T> where T : ProductBase
{
    public async Task<T?> GetAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<T?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Barcode == barcode, cancellationToken);
    }

    public async Task<PagedList<T>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        await InitializeDbContextAsync();

        var query = NonDeletedQuery;

        // Apply search filter
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(x => x.Name.Contains(filter.Search) || x.Barcode.Contains(filter.Search));

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            query = query.ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            // Default sorting if no sort criteria is provided
            query = query.OrderByDescending(x => x.LastModifiedDate);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        query = query.Skip(filter.Skip ?? 0).Take(filter.Take ?? 100);

        var list = await query.ToListAsync(cancellationToken);

        return new PagedList<T> { Data = list, Skip = filter.Skip ?? 0, Take = filter.Take ?? 100, Total = totalRecords };
    }

    public async Task<List<T>> GetPendingItemsAsync(DateTime checkpointDate,
        CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        List<T> list = [];

        // ClientSide pending items
        if (typeof(T).IsAssignableTo(typeof(ITrackableEntity)))
        {
            list = await AllQuery.ToListAsync(cancellationToken);

            list = list.AsQueryable()
                .Where($"{nameof(ITrackableEntity.Status)}<>{(int)ModifyStatus.Synced}")
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToList();
        }

        // Serverside pending items
        if (typeof(T).IsAssignableTo(typeof(ISoftDeleteEntity)))
        {
            list = await AllQuery.ToListAsync(cancellationToken);

            list = await AllQuery
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        return list;
    }
}