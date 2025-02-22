using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using GoldEx.Sdk.Server.Infrastructure.Extensions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
public class ProductRepository(GoldExDbContext context) : RepositoryBase<Product>(context), IProductRepository
{
    public Task<Product?> GetAsync(ProductId id, CancellationToken cancellationToken = default)
        => Query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Product?> GetAsync(string barcode, CancellationToken cancellationToken = default)
        => Query.FirstOrDefaultAsync(x => x.Barcode == barcode, cancellationToken);

    public async Task<PagedList<Product>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;
        
        var query = Query.AsNoTracking();

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
            query = query.OrderByDescending(x => x.CreatedAt);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        query = query.Skip(filter.Skip ?? 0).Take(filter.Take ?? 100);

        var list = await query.ToListAsync(cancellationToken);

        return new PagedList<Product> { Data = list, Skip = filter.Skip ?? 0, Take = filter.Take ?? 100, Total = totalRecords };

    }
}