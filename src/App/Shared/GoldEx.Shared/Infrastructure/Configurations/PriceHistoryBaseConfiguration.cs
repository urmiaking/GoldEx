using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Shared.Infrastructure.Configurations;

public static class PriceHistoryBaseConfiguration
{
    public static void Configure<T>(EntityTypeBuilder<T> builder) 
        where T : PriceHistoryBase
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