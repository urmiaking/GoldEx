using GoldEx.Client.Offline.Domain.SettingsAggregate;
using GoldEx.Shared.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Client.Offline.Infrastructure.Configurations;

public class SettingsConfigurations : IEntityTypeConfiguration<Settings>
{
    public void Configure(EntityTypeBuilder<Settings> builder)
    {
        SettingsBaseConfiguration.Configure(builder);
    }
}