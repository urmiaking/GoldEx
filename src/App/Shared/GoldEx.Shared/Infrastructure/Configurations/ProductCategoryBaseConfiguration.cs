using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Shared.Infrastructure.Configurations;

public static class ProductCategoryBaseConfiguration
{
    public static void Configure<T>(EntityTypeBuilder<T> builder) where T : ProductCategoryBase
    {
        builder.ToTable("ProductCategories");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new ProductCategoryId(value));

        builder.Property(x => x.Title)
            .HasMaxLength(50)
            .IsRequired();
    }
}