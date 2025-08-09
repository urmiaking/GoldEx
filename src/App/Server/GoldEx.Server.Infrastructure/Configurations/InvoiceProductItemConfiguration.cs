using GoldEx.Server.Domain.InvoiceProductItemAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class InvoiceProductItemConfiguration : IEntityTypeConfiguration<InvoiceProductItem>
{
    public void Configure(EntityTypeBuilder<InvoiceProductItem> builder)
    {
        builder.ToTable("InvoiceProductItems");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new InvoiceProductItemId(value));

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

        builder.HasOne(x => x.SellProduct)
            .WithOne(x => x.SellInvoiceProductItem)
            .HasForeignKey<InvoiceProductItem>(x => x.SellProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PurchaseProduct)
            .WithOne(x => x.PurchaseInvoiceProductItem)
            .HasForeignKey<InvoiceProductItem>(x => x.PurchaseProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.ProductItems)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PurchaseProductId)
            .IsUnique()
            .HasFilter("[PurchaseProductId] IS NOT NULL");

        builder.HasIndex(x => x.SellProductId)
            .IsUnique()
            .HasFilter("[SellProductId] IS NOT NULL");
    }
}