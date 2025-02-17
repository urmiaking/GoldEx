using GoldEx.Server.Domain.PriceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class PriceConfiguration : IEntityTypeConfiguration<Price>
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

        builder.Property(x => x.PriceType)
            .IsRequired();

        builder.OwnsMany(x => x.PriceHistories, Configure);
    }

    private void Configure(OwnedNavigationBuilder<Price, PriceHistory> builder)
    {
        builder.ToTable("PriceHistories");

        builder.Property(x => x.DailyChangeRate)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.LastUpdate)
            .HasMaxLength(50)
            .IsRequired();
    }
}