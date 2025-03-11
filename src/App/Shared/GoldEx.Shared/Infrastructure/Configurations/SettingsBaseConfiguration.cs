using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Shared.Infrastructure.Configurations;

public static class SettingsBaseConfiguration
{
    public static void Configure<T>(EntityTypeBuilder<T> builder)
        where T : SettingsBase
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
    }
}