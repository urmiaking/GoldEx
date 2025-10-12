using GoldEx.Server.Domain.TransactionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new TransactionId(value));

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.BaseCurrencyAmount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LedgerAccount)
            .WithMany()
            .HasForeignKey(x => x.LedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Invoice)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentVoucher)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.PaymentVoucherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InvoicePayment)
            .WithMany()
            .HasForeignKey(x => x.InvoicePaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MeltingBatch)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.MeltingBatchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}