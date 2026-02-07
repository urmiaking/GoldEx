using GoldEx.Server.Domain.LicensePaymentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class LicensePaymentConfiguration : IEntityTypeConfiguration<LicensePayment>
{
    public void Configure(EntityTypeBuilder<LicensePayment> builder)
    {
        builder.ToTable("AppLicensePayments");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new LicensePaymentId(id));

        builder.Property(x => x.PaymentReference)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.PaymentDescription)
            .HasMaxLength(1000);
    }
}