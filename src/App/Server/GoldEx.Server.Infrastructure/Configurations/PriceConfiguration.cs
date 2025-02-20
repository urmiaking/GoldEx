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

        builder.Property(x => x.IconUrl)
            .HasMaxLength(500);

        builder.Property(x => x.MarketType)
            .IsRequired();

        builder.HasMany(x => x.PriceHistories)
            .WithOne(x => x.Price)
            .HasForeignKey(x => x.PriceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}