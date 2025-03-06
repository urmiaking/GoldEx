using GoldEx.Client.Offline.Domain.CheckpointAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Client.Offline.Infrastructure.Configurations;

public class CheckpointConfiguration : IEntityTypeConfiguration<Checkpoint>
{
    public void Configure(EntityTypeBuilder<Checkpoint> builder)
    {
        builder.ToTable("Checkpoints");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new CheckpointId(value));

        builder.Property(x => x.EntityName)
            .IsRequired().HasMaxLength(50);
    }
}