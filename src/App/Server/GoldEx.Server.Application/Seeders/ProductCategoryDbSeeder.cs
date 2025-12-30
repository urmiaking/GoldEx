using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductCategories;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

//[ScopedService] // Uncomment this line to register the seeder in the DI container
internal sealed class ProductCategoryDbSeeder(IProductCategoryRepository repository, ILogger<ProductCategoryDbSeeder> logger) : IDbSeeder
{
    public int Order => 60;
    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        var productCategoryCount = await repository.CountAsync(new ProductCategoriesDefaultSpecification(), cancellationToken);

        if (productCategoryCount > 0)
            return;

        var defaultCategories = new List<ProductCategory>
        {
            ProductCategory.Create("انگشتر", "01"),
            ProductCategory.Create("النگو", "02"),
            ProductCategory.Create("دستبند", "03"),
            ProductCategory.Create("گوشواره", "04"),
            ProductCategory.Create("سرویس کامل", "05"),
            ProductCategory.Create("گردنبند", "06"),
            ProductCategory.Create("نیم‌ست", "07")
        };

        await repository.CreateRangeAsync(defaultCategories, cancellationToken);

        logger.LogInformation("Seeded {Count} product categories.", defaultCategories.Count);
    }
}