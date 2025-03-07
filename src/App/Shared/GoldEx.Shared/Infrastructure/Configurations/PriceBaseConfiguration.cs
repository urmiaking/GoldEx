using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Shared.Infrastructure.Configurations;

public static class PriceBaseConfiguration
{
    public static void Configure<TPrice, TPriceHistory>(EntityTypeBuilder<TPrice> builder)
    where TPrice : PriceBase<TPriceHistory>
    where TPriceHistory : PriceHistoryBase
    {
        builder.ToTable("Prices");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new PriceId(value));

        builder.Property(x => x.Title)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.IconFile);

        builder.Property(x => x.MarketType)
            .IsRequired();

        builder.OwnsOne(x => x.PriceHistory, Config);
    }

    private static void Config<TPrice, TPriceHistory>(OwnedNavigationBuilder<TPrice, TPriceHistory> builder)
        where TPrice : PriceBase<TPriceHistory> where TPriceHistory : PriceHistoryBase
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
    }
}