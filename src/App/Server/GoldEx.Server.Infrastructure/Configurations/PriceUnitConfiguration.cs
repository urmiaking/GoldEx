using GoldEx.Server.Domain.PriceUnitAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class PriceUnitConfiguration : IEntityTypeConfiguration<PriceUnit>
{
    public void Configure(EntityTypeBuilder<PriceUnit> builder)
    {
        builder.ToTable("PriceUnits");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new PriceUnitId(value));

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasOne(x => x.Price)
            .WithOne()
            .HasForeignKey<PriceUnit>(x => x.PriceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}