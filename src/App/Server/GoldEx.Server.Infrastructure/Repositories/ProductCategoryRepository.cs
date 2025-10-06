using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class ProductCategoryRepository(GoldExDbContext dbContext) : RepositoryBase<ProductCategory>(dbContext), IProductCategoryRepository
{
    public async Task<string> GetLastPrefixCodeAsync(CancellationToken cancellationToken = default)
    {
        var lastCode = await Query
            .OrderByDescending(pc => pc.PrefixCode)
            .Select(pc => pc.PrefixCode)
            .FirstOrDefaultAsync(cancellationToken);

        return lastCode ?? "00";
    }
}