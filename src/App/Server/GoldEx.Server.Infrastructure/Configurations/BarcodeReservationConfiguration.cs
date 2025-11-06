using GoldEx.Server.Domain.BarcodeReservationAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class BarcodeReservationConfiguration : IEntityTypeConfiguration<BarcodeReservation>
{
    public void Configure(EntityTypeBuilder<BarcodeReservation> builder)
    {
        builder.ToTable("BarcodeReservations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, v => new BarcodeReservationId(v));

        builder.Property(x => x.Prefix)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.Barcode)
            .IsRequired()
            .HasMaxLength(16); // allow some headroom; current is 8

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasOne<Invoice>()
            .WithMany()
            .HasForeignKey(x => x.InvoiceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull); // consider SetNull if you want to retain reservation history

        // Unique on barcode across all reservations
        builder.HasIndex(x => x.Barcode)
            .IsUnique();

        // Supports fast queries for issuing and cleanup
        builder.HasIndex(x => new { x.Prefix, x.Status, x.ExpiresAt })
            .IsUnique(false);

        // Optional: if you frequently query latest by prefix
        builder.HasIndex(x => x.Prefix)
            .IsUnique(false);
    }
}