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

        builder.HasOne(x => x.BasePriceUnit)
            .WithMany()
            .HasForeignKey(x => x.BasePriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UnpaidPriceUnit)
            .WithMany()
            .HasForeignKey(x => x.UnpaidPriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.BasePriceUnit).AutoInclude();
        builder.Navigation(x => x.PriceUnit).AutoInclude();
        builder.Navigation(x => x.UnpaidPriceUnit).AutoInclude();

        builder.OwnsMany(x => x.Discounts, Configure);
        builder.OwnsMany(x => x.ExtraCosts, Configure);
        builder.OwnsMany(x => x.ProductItems, Configure);
        builder.OwnsMany(x => x.CoinItems, Configure);
        builder.OwnsMany(x => x.CurrencyItems, Configure);
        builder.OwnsMany(x => x.UsedProducts, Configure);
    }

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceUsedProduct> builder)
    {
        builder.ToTable("InvoiceUsedProducts");

        builder.WithOwner(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId);

        builder.HasKey(x => new { x.Id, x.InvoiceId });

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new InvoiceUsedProductId(value));

        builder.Property(x => x.Description)
            .HasMaxLength(100);

        builder.Property(x => x.Weight)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.GramPrice)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ExtraCostsAmount)
            .HasPrecision(36, 10);

        builder.Property(x => x.FinenessDeductionRate)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.ItemAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ItemFinalAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceCurrencyItem> builder)
    {
        builder.ToTable("InvoiceCurrencyItems");

        builder.WithOwner(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId);

        builder.HasKey(x => new { x.Id, x.InvoiceId });

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new InvoiceCurrencyItemId(value));

        builder.Property(x => x.TaxPercent)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.ProfitPercent)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.ItemRawAmount)
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

        builder.Property(x => x.UnitPrice)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FinancialAccount)
            .WithMany()
            .HasForeignKey(x => x.FinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceCoinItem> builder)
    {
        builder.ToTable("InvoiceCoinItems");

        builder.WithOwner(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId);

        builder.HasKey(x => new { x.Id, x.InvoiceId });

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new InvoiceCoinItemId(value));

        builder.Property(x => x.ProfitPercent)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.ItemRawAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ItemProfitAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ItemFinalAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.Coin)
            .WithMany()
            .HasForeignKey(x => x.CoinId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceProductItem> builder)
    {
        builder.ToTable("InvoiceProductItems");

        builder.WithOwner(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId);

        builder.HasKey(x => new { x.Id, x.InvoiceId });

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new InvoiceProductItemId(value));

        builder.Property(x => x.GramPrice)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.TaxPercent)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.ProfitPercent)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.Property(x => x.CostPrice)
            .HasPrecision(36, 10);

        builder.Property(x => x.CostPriceExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.TotalWeight)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.SaleWage)
            .HasPrecision(36, 10);

        builder.Property(x => x.SaleWagePriceUnitExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.StonePriceUnitExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.ItemRawAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ItemStoneAmount)
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

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostPriceUnit)
            .WithMany()
            .HasForeignKey(x => x.CostPriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SaleWagePriceUnit)
            .WithMany()
            .HasForeignKey(x => x.SaleWagePriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.CostPriceUnit).AutoInclude();
        builder.Navigation(x => x.SaleWagePriceUnit).AutoInclude();
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

        builder.Navigation(x => x.PriceUnit).AutoInclude();
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

        builder.Navigation(x => x.PriceUnit).AutoInclude();
    }
}