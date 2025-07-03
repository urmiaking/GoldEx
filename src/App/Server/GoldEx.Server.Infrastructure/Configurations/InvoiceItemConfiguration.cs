using GoldEx.Server.Domain.InvoiceItemAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class InvoiceItemConfiguration : IEntityTypeConfiguration<InvoiceItem>
{
    public void Configure(EntityTypeBuilder<InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new InvoiceItemId(value));

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

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}