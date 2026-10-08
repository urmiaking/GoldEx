using GoldEx.Server.Domain.SmartTrayAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class SmartTrayConfiguration : IEntityTypeConfiguration<SmartTray>
{
    public void Configure(EntityTypeBuilder<SmartTray> builder)
    {
        builder.ToTable("SmartTrays");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => new SmartTrayId(value));

        builder.Property(x => x.TrayNumber)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ClerkName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DiscrepancyNotes)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.StoreId, x.TrayNumber });

        builder.HasIndex(x => new { x.StoreId, x.Status });

        builder.HasMany(x => x.Items)
            .WithOne(x => x.SmartTray)
            .HasForeignKey(x => x.SmartTrayId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items).AutoInclude();
    }
}
