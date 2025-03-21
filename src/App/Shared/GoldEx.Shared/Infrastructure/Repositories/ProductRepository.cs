using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Extensions;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;

namespace GoldEx.Shared.Infrastructure.Repositories;

public class ProductRepository<TProduct, TCategory, TGemStone>(
    IGoldExDbContextFactory factory) : RepositoryBase<TProduct>(factory),
    IProductRepository<TProduct, TCategory, TGemStone>
    where TProduct : ProductBase<TCategory, TGemStone>
    where TCategory : ProductCategoryBase
    where TGemStone : GemStoneBase
{
    public async Task<TProduct?> GetAsync(ProductId id, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery
            .Include(x => x.ProductCategory)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TProduct?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery
            .Include(x => x.ProductCategory)
            .FirstOrDefaultAsync(x => x.Barcode == barcode, cancellationToken);
    }

    public async Task<PagedList<TProduct>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        await InitializeDbContextAsync();

        var query = NonDeletedQuery
            .Include(x => x.ProductCategory)
            .AsQueryable();

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

        return new PagedList<TProduct> { Data = list, Skip = filter.Skip ?? 0, Take = filter.Take ?? 100, Total = totalRecords };
    }

    public async Task<List<TProduct>> GetPendingItemsAsync(DateTime checkpointDate,
        CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        // ClientSide pending items
        if (typeof(TProduct).IsAssignableTo(typeof(ITrackableEntity)))
        {
            return await AllQuery
                .Where($"{nameof(ITrackableEntity.Status)}<>{(int)ModifyStatus.Synced}")
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        // Serverside pending items
        if (typeof(TProduct).IsAssignableTo(typeof(ISoftDeleteEntity)))
        {
            return await AllQuery
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        return [];
    }

    public async Task<bool> CheckCategoryUsedAsync(ProductCategoryId categoryId, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await AllQuery.AnyAsync(x => x.ProductCategoryId == categoryId, cancellationToken);
    }
}