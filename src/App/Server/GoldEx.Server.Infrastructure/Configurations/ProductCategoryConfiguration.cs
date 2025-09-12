using GoldEx.Server.Domain.ProductCategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new ProductCategoryId(value));

        builder.Property(x => x.Title)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PrefixCode)
            .HasMaxLength(2)
            .IsRequired();

        builder.HasIndex(x => x.Title)
            .IsUnique();

        builder.HasIndex(x => x.PrefixCode)
            .IsUnique();
    }
}