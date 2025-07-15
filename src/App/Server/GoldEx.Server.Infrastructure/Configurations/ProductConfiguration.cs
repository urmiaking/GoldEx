using GoldEx.Server.Domain.ProductAggregate;
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
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.Wage)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.ProductCategory)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WagePriceUnit)
            .WithMany()
            .HasForeignKey(x => x.WagePriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Barcode)
            .IsUnique();

        builder.HasIndex(x => x.Name);

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
            .HasPrecision(36, 10)
            .IsRequired();
    }
}