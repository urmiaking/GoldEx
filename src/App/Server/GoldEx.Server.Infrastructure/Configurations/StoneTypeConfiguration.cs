using GoldEx.Server.Domain.StoneTypeAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class StoneTypeConfiguration : IEntityTypeConfiguration<StoneType>
{
    public void Configure(EntityTypeBuilder<StoneType> builder)
    {
        builder.ToTable("StoneTypes");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new StoneTypeId(id));

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.EnTitle)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(4);

        builder.HasIndex(x => x.Title)
            .IsUnique();

        builder.HasIndex(x => x.EnTitle)
            .IsUnique();

        builder.HasIndex(x => x.Symbol)
            .IsUnique();
    }
}