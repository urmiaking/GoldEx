using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using GoldEx.Shared.Domain.Aggregates.ProductCategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Shared.Infrastructure.Configurations;

public static class ProductBaseConfiguration
{
    public static void Configure<TProduct, TCategory, TGemStone>(EntityTypeBuilder<TProduct> builder)
        where TProduct : ProductBase<TCategory, TGemStone>
        where TCategory : ProductCategoryBase
        where TGemStone : GemStoneBase
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
            .IsRequired();

        builder.Property(x => x.ProductCategoryId)
            .HasConversion(id => id.Value,
                value => new ProductCategoryId(value));

        builder.HasOne(x => x.ProductCategory)
            .WithMany()
            .HasForeignKey(x => x.ProductCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.GemStones, Configure<TProduct, TCategory, TGemStone>);


    }

    private static void Configure<TProduct, TCategory, TGemStone>(OwnedNavigationBuilder<TProduct, TGemStone> builder) 
        where TProduct : ProductBase<TCategory, TGemStone>
        where TGemStone : GemStoneBase
        where TCategory : ProductCategoryBase
    {
        builder.ToTable("GemStones");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new GemStoneId(value));

        builder.Property(x => x.Type)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Carat)
            .IsRequired();
    }
}