using GoldEx.Server.Domain.CoinInstanceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class CoinInstanceConfiguration : IEntityTypeConfiguration<CoinInstance>
{
    public void Configure(EntityTypeBuilder<CoinInstance> builder)
    {
        builder.ToTable("CoinInstances");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new CoinInstanceId(id));

        builder.Property(x => x.Barcode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Weight)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.Fineness)
            .HasPrecision(9, 6)
            .IsRequired();

        builder.HasIndex(x => x.Barcode)
            .IsUnique();

        builder.HasOne(x => x.Coin)
            .WithMany()
            .HasForeignKey(x => x.CoinId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.OwnsOne(x => x.CoinInstancePackage, Configure);
    }

    private void Configure(OwnedNavigationBuilder<CoinInstance, CoinInstancePackage> builder)
    {
        builder.ToTable("CoinInstancePackages");

        builder.Property(x => x.VacuumedWeight)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.CardColor)
            .HasMaxLength(30);

        builder.Property(x => x.StandardCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasOne(x => x.Issuer)
            .WithMany()
            .HasForeignKey(x => x.IssuerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}