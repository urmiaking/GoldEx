using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class ProductRepository(GoldExDbContext dbContext) : RepositoryBase<Product>(dbContext), IProductRepository
{
    public async Task<string?> GetLastBarcodeWithPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var lastProduct = await Query
            .AsNoTracking()
            .Where(p => p.Barcode.StartsWith(prefix))
            .OrderByDescending(p => p.Barcode)
            .FirstOrDefaultAsync(cancellationToken);

        return lastProduct?.Barcode;
    }
}