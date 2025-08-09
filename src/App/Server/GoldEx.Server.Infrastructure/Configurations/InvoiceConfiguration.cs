using GoldEx.Server.Domain.InvoiceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new InvoiceId(value));

        builder.Property(x => x.UnpaidAmountExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(36, 10);

        builder.HasIndex(x => new { x.InvoiceNumber, x.InvoiceType })
            .IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UnpaidPriceUnit)
            .WithMany()
            .HasForeignKey(x => x.UnpaidPriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.Discounts, Configure);
        builder.OwnsMany(x => x.ExtraCosts, Configure);
        builder.OwnsMany(x => x.Items, Configure);
    }

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceItemBase> builder)
    {
        builder.ToTable("InvoiceProductItems");

        builder.HasDiscriminator<string>("ItemType")
            .HasValue<InvoiceProductItem>("Product")
            .HasValue<InvoiceCoinItem>("Coin")
            .HasValue<InvoiceCurrencyItem>("Currency");

        builder.Property<decimal>(nameof(InvoiceProductItem.GramPrice))
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property<decimal>(nameof(InvoiceProductItem.TaxPercent))
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property<decimal>(nameof(InvoiceProductItem.ProfitPercent))
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(nameof(InvoiceProductItem.ExchangeRate))
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

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceExtraCost> builder)
    {
        builder.ToTable("InvoiceExtraCosts");

        builder.Property(x => x.Amount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceDiscount> builder)
    {
        builder.ToTable("InvoiceDiscounts");

        builder.Property(x => x.Amount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}