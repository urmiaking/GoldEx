using GoldEx.Server.Domain.LedgerAccountAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class LedgerAccountConfiguration : IEntityTypeConfiguration<LedgerAccount>
{
    public void Configure(EntityTypeBuilder<LedgerAccount> builder)
    {
        builder.ToTable("LedgerAccounts");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new LedgerAccountId(value));

        builder.Property(x => x.Title)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(x => x.Title)
            .IsUnique();

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.LedgerAccounts)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ParentAccount)
            .WithMany()
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CustomerId, x.ParentAccountId, x.PriceUnitId })
            .IsUnique();
    }
}