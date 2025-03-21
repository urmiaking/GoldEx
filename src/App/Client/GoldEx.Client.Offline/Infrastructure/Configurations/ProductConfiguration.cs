using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Client.Offline.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Client.Offline.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        ProductBaseConfiguration.Configure<Product, ProductCategory, GemStone>(builder);
    }
}