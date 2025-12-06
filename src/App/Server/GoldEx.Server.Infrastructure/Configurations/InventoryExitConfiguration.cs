using GoldEx.Server.Domain.InventoryExitAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class InventoryExitConfiguration : IEntityTypeConfiguration<InventoryExit>
{
    public void Configure(EntityTypeBuilder<InventoryExit> builder)
    {
        builder.ToTable("InventoryExits");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new InventoryExitId(id));

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}