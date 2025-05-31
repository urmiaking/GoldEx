using GoldEx.Server.Domain.SettingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class SettingsConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new SettingsId(value));

        builder.Property(x => x.Address)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.InstitutionName)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.TaxPercent)
            .HasPrecision(9, 6);

        builder.Property(x => x.GoldProfitPercent)
            .HasPrecision(9, 6);

        builder.Property(x => x.JewelryProfitPercent)
            .HasPrecision(9, 6);
    }
}