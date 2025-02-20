using GoldEx.Server.Domain.PriceHistoryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class PriceHistoryConfiguration : IEntityTypeConfiguration<PriceHistory>
{
    public void Configure(EntityTypeBuilder<PriceHistory> builder)
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