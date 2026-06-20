using GoldEx.Server.Domain.CheckPaymentAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal sealed class CheckPaymentConfiguration : IEntityTypeConfiguration<CheckPayment>
{
    public void Configure(EntityTypeBuilder<CheckPayment> builder)
    {
        builder.ToTable("CheckPayments");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new CheckPaymentId(id));

        builder.Property(x => x.Number)
            .HasMaxLength(100);

        builder.Property(x => x.SayadiCode)
            .HasMaxLength(100);

        builder.Property(x => x.DueDate)
            .IsRequired();

        builder.HasOne(x => x.InvoicePayment)
            .WithOne(x => x.CheckPayment)
            .HasForeignKey<CheckPayment>(x => x.InvoicePaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Issuer)
            .WithMany()
            .HasForeignKey(x => x.IssuerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.IssuerFinancialAccount)
            .WithMany()
            .HasForeignKey(x => x.IssuerFinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsMany(x => x.ChangeLogs, Configure);

        builder.HasIndex(x => x.InvoicePaymentId)
            .IsUnique();

        builder.HasIndex(x => new { x.StoreId, x.Number })
            .IsUnique();

        builder.HasIndex(x => new { x.StoreId, x.SayadiCode })
            .IsUnique();
    }

    private void Configure(OwnedNavigationBuilder<CheckPayment, CheckPaymentChangeLog> builder)
    {
        builder.ToTable("CheckPaymentChangeLogs");

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasOne(x => x.TargetFinancialAccount)
            .WithMany()
            .HasForeignKey(x => x.TargetFinancialAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.TargetFinancialAccount)
            .AutoInclude();
    }
}