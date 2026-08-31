using GoldEx.Server.Domain.ProductAttributeAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
{
    public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
    {
        builder.ToTable("CategoryAttributes");

        builder.HasKey(x => new { x.ProductCategoryId, x.ProductAttributeId });

        builder.Property(x => x.ProductCategoryId)
            .HasConversion(id => id.Value,
                value => new ProductCategoryId(value));

        builder.Property(x => x.ProductAttributeId)
            .HasConversion(id => id.Value,
                value => new ProductAttributeId(value));

        builder.Property(x => x.IsRequired)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.ShowInFilter)
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasOne(x => x.ProductCategory)
            .WithMany(x => x.Attributes)
            .HasForeignKey(x => x.ProductCategoryId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProductAttribute)
            .WithMany()
            .HasForeignKey(x => x.ProductAttributeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.StoreId);
    }
}
