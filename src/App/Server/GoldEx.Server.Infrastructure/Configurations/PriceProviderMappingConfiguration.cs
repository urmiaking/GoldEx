using GoldEx.Server.Domain.PriceProviderMappingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal sealed class PriceProviderMappingConfiguration : IEntityTypeConfiguration<PriceProviderMapping>
{
    public void Configure(EntityTypeBuilder<PriceProviderMapping> builder)
    {
        builder.ToTable("PriceProviderMappings");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, val => new PriceProviderMappingId(val));

        builder.Property(x => x.ProviderType)
            .IsRequired();

        builder.Property(x => x.ProviderSymbol)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        // TODO: need to be reviewed
        builder.HasOne(x => x.Price)
            .WithMany()
            .HasForeignKey(x => x.PriceId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}