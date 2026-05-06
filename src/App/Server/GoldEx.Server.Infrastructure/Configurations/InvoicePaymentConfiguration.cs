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
            .HasPrecision(38, 18);

        builder.Property(x => x.GoldFineness)
            .HasPrecision(9, 6);

        builder.Property(x => x.FinalAmount)
            .HasPrecision(36, 10)
            .IsRequired();

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

        builder.HasOne(x => x.LedgerAccount)
            .WithMany()
            .HasForeignKey(x => x.LedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.InvoicePayments)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourcePayment)
            .WithMany()
            .HasForeignKey(x => x.SourcePaymentId)
            .OnDelete(DeleteBehavior.Restrict); // We want the target payment be deleted if the source payment gone

        builder.HasOne(x => x.TargetInvoice)
            .WithMany()
            .HasForeignKey(x => x.TargetInvoiceId)
            .OnDelete(DeleteBehavior.Restrict); // Caution: We do not want to delete the payment if the target invoice id is deleted.

        builder.Navigation(x => x.PriceUnit).AutoInclude();
        builder.Navigation(x => x.CheckPayment).AutoInclude();
    }
}