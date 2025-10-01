using GoldEx.Server.Domain.InvoicePaymentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class InvoicePaymentConfiguration : IEntityTypeConfiguration<InvoicePayment>
{
    public void Configure(EntityTypeBuilder<InvoicePayment> builder)
    {
        builder.ToTable("InvoicePayments");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new InvoicePaymentId(value));

        builder.Property(x => x.Amount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentVoucher)
            .WithOne()
            .HasForeignKey<InvoicePayment>(x => x.PaymentVoucherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceFinancialAccount)
            .WithMany()
            .HasForeignKey(x => x.SourceFinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.InvoicePayments)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.PriceUnit).AutoInclude();
    }
}