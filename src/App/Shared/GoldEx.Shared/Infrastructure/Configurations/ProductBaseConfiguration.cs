using GoldEx.Shared.Domain.Aggregates.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Shared.Infrastructure.Configurations;

public static class ProductBaseConfiguration
{
    public static void Configure<T>(EntityTypeBuilder<T> builder)
        where T : ProductBase
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
    }
}