using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new ProductId(value));

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Barcode)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ProductCategoryId)
            .HasConversion(id => id.Value,
                value => new ProductCategoryId(value));

        builder.HasOne(x => x.ProductCategory)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.GemStones, Configure);
    }

    private static void Configure(OwnedNavigationBuilder<Product, GemStone> builder)
    {
        builder.ToTable("GemStones");

        builder.HasKey(x => new { x.Code, x.ProductId });

        builder.Property(x => x.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Carat)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}