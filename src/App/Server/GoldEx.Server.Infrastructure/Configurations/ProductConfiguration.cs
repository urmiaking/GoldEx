using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.StoneTypeAggregate;
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

        builder.Property(x => x.Fineness)
            .HasPrecision(9, 6);

        builder.HasOne(x => x.ProductCategory)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.WagePriceUnit)
            .WithMany()
            .HasForeignKey(x => x.WagePriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.StonePriceUnit)
            .WithMany()
            .HasForeignKey(x => x.StonePriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.WagePriceUnit).AutoInclude();
        builder.Navigation(x => x.StonePriceUnit).AutoInclude();
        builder.Navigation(x => x.ProductCategory).AutoInclude();

        builder.HasIndex(x => new { x.StoreId, x.Barcode })
            .IsUnique();

        builder.HasIndex(x => x.Name);

        builder.OwnsMany(x => x.GemStones, Configure);
        builder.OwnsOne(x => x.MoltenGold, Configure);
    }

    private void Configure(OwnedNavigationBuilder<Product, MoltenGold> builder)
    {
        builder.ToTable("MoltenGolds");

        builder.Property(x => x.AssayNumber)
            .HasMaxLength(50);

        builder.HasOne(x => x.Assayer)
            .WithMany()
            .HasForeignKey(x => x.AssayerId)
            .OnDelete(DeleteBehavior.Restrict);
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

        builder.Property(x => x.Cost)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.StoneType)
            .WithMany()
            .HasForeignKey(x => x.StoneTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.StoneType).AutoInclude();
    }
}