using GoldEx.Server.Domain.InventoryEntryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class InventoryEntryConfiguration : IEntityTypeConfiguration<InventoryEntry>
{
    public void Configure(EntityTypeBuilder<InventoryEntry> builder)
    {
        builder.ToTable("InventoryEntries");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new InventoryEntryId(id));
    }
}