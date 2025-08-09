using GoldEx.Server.Domain.InvoiceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class InvoiceProductItemConfiguration : IEntityTypeConfiguration<InvoiceProductItem>
{
    public void Configure(EntityTypeBuilder<InvoiceProductItem> builder)
    {
        builder.ToTable("InvoiceProductItems");

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.GramPrice)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.TaxPercent)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.ProfitPercent)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.ItemRawAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ItemWageAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ItemProfitAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ItemTaxAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ItemFinalAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ProductId)
            .IsUnique()
            .HasFilter("[SellProductId] IS NOT NULL");
    }
}