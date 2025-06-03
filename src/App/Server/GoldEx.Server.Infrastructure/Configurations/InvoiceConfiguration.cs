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

        builder.Property(x => x.AdditionalPrices)
            .HasPrecision(36, 10);

        builder.Property(x => x.Discount)
            .HasPrecision(36, 10);

        builder.HasOne(x => x.Customer)
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.Items, Configure);
        builder.OwnsOne(x => x.InvoiceDebt, Configure);
    }

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceItem> builder)
    {
        builder.ToTable("InvoiceItems");

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.Tax)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private void Configure(OwnedNavigationBuilder<Invoice, InvoiceDebt> builder)
    {
        builder.ToTable("InvoiceDebts");

        builder.Property(x => x.Amount)
            .HasPrecision(36, 10)
            .IsRequired();
    }
}