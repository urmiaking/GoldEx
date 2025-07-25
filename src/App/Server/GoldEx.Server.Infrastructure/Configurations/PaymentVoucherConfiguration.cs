using GoldEx.Server.Domain.PaymentVoucherAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class PaymentVoucherConfiguration : IEntityTypeConfiguration<PaymentVoucher>
{
    public void Configure(EntityTypeBuilder<PaymentVoucher> builder)
    {
        builder.ToTable("PaymentVouchers");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new PaymentVoucherId(value));

        builder.Property(x => x.Amount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(36, 10);

        builder.Property(x => x.Description)
            .HasMaxLength(150)
            .IsRequired();

        builder.HasIndex(x => x.VoucherNumber)
            .IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.PaymentVouchers)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.BankAccount)
            .WithMany()
            .HasForeignKey(x => x.BankAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AmountPriceUnit)
            .WithMany()
            .HasForeignKey(x => x.AmountPriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.VoucherPriceUnit)
            .WithMany()
            .HasForeignKey(x => x.VoucherPriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}