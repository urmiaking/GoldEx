using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.ProductAttributeAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.ProductAttributes;
using GoldEx.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal sealed class ProductAttributeDbSeeder(
    IProductAttributeRepository repository,
    ILogger<ProductAttributeDbSeeder> logger) : IDbSeeder
{
    public int Order => 85;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        var count = await repository.CountAsync(new ProductAttributesDefaultSpecification(), cancellationToken);

        if (count > 0)
            return;

        var defaultAttributes = new List<ProductAttribute>
        {
            ProductAttribute.Create("سایز", "سایز", ProductAttributeDataType.Text, "سایز ۱, سایز ۲, سایز ۳, سایز ۴", "سایز استاندارد قطعه طلا یا النگو"),
            ProductAttribute.Create("قطر", "میلی‌متر", ProductAttributeDataType.Number, null, "قطر داخلی یا ضخامت قطعه به میلی‌متر"),
            ProductAttribute.Create("طول", "سانتی‌متر", ProductAttributeDataType.Number, null, "طول کل دستبند یا گردنبند به سانتی‌متر"),
            ProductAttribute.Create("رنگ طلا", null, ProductAttributeDataType.Select, "زرد, سفید, دو رنگ, رزگلد", "رنگ پایه و آبکاری طلا"),
            ProductAttribute.Create("سبک طراحی", null, ProductAttributeDataType.Select, "کارتیه, ون‌کلیف, رولکس, تیفانی, هرمس, مینیمال, سنتی و تراش", "کالکشن یا سبک دیزاین"),
            ProductAttribute.Create("نوع قفل", null, ProductAttributeDataType.Select, "مدبر (طوطی), حلقه‌ای, چفتی, فشاری, کشویی", "نوع قفل و بست قطعه")
        };

        await repository.CreateRangeAsync(defaultAttributes, cancellationToken);

        logger.LogInformation("Seeded {Count} product attributes.", defaultAttributes.Count);
    }
}
