using GoldEx.Server.Domain.CoinAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class CoinConfiguration : IEntityTypeConfiguration<Coin>
{
    public void Configure(EntityTypeBuilder<Coin> builder)
    {
        builder.ToTable("Coins");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new CoinId(value));

        builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Weight)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.Fineness)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.HasOne(x => x.Price)
            .WithMany()
            .HasForeignKey(x => x.PriceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LedgerAccount)
            .WithMany()
            .HasForeignKey(x => x.LedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}