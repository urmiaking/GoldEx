using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.SmartTrayAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class SmartTrayItemConfiguration : IEntityTypeConfiguration<SmartTrayItem>
{
    public void Configure(EntityTypeBuilder<SmartTrayItem> builder)
    {
        builder.ToTable("SmartTrayItems");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SmartTrayItemId(value));

        builder.Property(x => x.SmartTrayId)
            .HasConversion(id => id.Value, value => new SmartTrayId(value));

        builder.Property(x => x.ProductId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new ProductId(value.Value) : null);

        builder.Property(x => x.CoinInstanceId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new CoinInstanceId(value.Value) : null);

        builder.Property(x => x.InvoiceId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new InvoiceId(value.Value) : null);

        builder.Property(x => x.Barcode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.CategoryTitle)
            .HasMaxLength(150);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Weight)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Fineness)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(x => x.Wage)
            .HasPrecision(36, 10);

        builder.HasIndex(x => new { x.SmartTrayId, x.Barcode });

        builder.HasIndex(x => new { x.StoreId, x.Barcode });
    }
}
