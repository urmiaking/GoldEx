using GoldEx.Server.Domain.PriceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class PriceConfiguration : IEntityTypeConfiguration<Price>
{
    public void Configure(EntityTypeBuilder<Price> builder)
    {
        builder.ToTable("Prices");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new PriceId(value));

        builder.Property(x => x.Title)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.MarketType)
            .IsRequired();

        builder.OwnsOne(x => x.PriceHistory, Config);
    }

    private static void Config(OwnedNavigationBuilder<Price, PriceHistory> builder)
    {
        builder.ToTable("PriceHistories");

        builder.Property(x => x.DailyChangeRate)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LastUpdate)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.CurrentValue)
            .HasPrecision(18, 2);
    }
}