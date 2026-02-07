using GoldEx.Server.Domain.AppLicenseAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class AppLicenseConfiguration : IEntityTypeConfiguration<AppLicense>
{
    public void Configure(EntityTypeBuilder<AppLicense> builder)
    {
        builder.ToTable("AppLicenses");

        builder.HasKey(x => x.LicenseId);

        builder.Property(x => x.VerificationKey)
            .HasMaxLength(500);
    }
}