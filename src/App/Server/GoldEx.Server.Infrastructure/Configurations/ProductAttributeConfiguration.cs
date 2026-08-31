using GoldEx.Server.Domain.ProductAttributeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new ProductAttributeId(value));

        builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(30);

        builder.Property(x => x.Options)
            .HasMaxLength(1000);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.StoreId, x.Title })
            .IsUnique();
    }
}
