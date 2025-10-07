using GoldEx.Server.Domain.MeltingBatchAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class MeltingBatchConfiguration : IEntityTypeConfiguration<MeltingBatch>
{
    public void Configure(EntityTypeBuilder<MeltingBatch> builder)
    {
        builder.ToTable("MeltingBatches");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value, id => new MeltingBatchId(id));

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.TotalWeight)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.HasOne(x => x.Assayer)
            .WithMany()
            .HasForeignKey(x => x.AssayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.ChangeLogs, Configure);
    }

    private void Configure(OwnedNavigationBuilder<MeltingBatch, MeltingBatchChangeLog> builder)
    {
        builder.ToTable("MeltingBatchChangeLogs");

        builder.Property(x => x.Description)
            .HasMaxLength(200);
    }
}