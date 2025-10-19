using GoldEx.Server.Domain.FinancialAccountAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class FinancialAccountConfiguration : IEntityTypeConfiguration<FinancialAccount>
{
    public void Configure(EntityTypeBuilder<FinancialAccount> builder)
    {
        builder.ToTable("FinancialAccounts");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new FinancialAccountId(value));

        builder.Property(x => x.HolderName)
            .HasMaxLength(100);

        builder.Property(x => x.BrokerName)
            .HasMaxLength(100);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.FinancialAccounts)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.LedgerAccount)
            .WithMany(x => x.FinancialAccounts)
            .HasForeignKey(x => x.LedgerAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(x => x.LocalAccount, Configure);
        builder.OwnsOne(x => x.InternationalAccount, Configure);
        builder.OwnsOne(x => x.CashAccount, Configure);

        builder.Navigation(x => x.LocalAccount).AutoInclude();
        builder.Navigation(x => x.InternationalAccount).AutoInclude();
        builder.Navigation(x => x.CashAccount).AutoInclude();
        builder.Navigation(x => x.PriceUnit).AutoInclude();
    }

    private void Configure(OwnedNavigationBuilder<FinancialAccount, CashAccount> builder)
    {
        builder.Property(x => x.Title)
            .HasMaxLength(50);
    }

    private void Configure(OwnedNavigationBuilder<FinancialAccount, InternationalBankAccount> builder)
    {
        builder.Property(x => x.SwiftBicCode)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.IbanNumber)
            .HasMaxLength(45)
            .IsRequired();

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();
    }

    private void Configure(OwnedNavigationBuilder<FinancialAccount, LocalBankAccount> builder)
    {
        builder.Property(x => x.CardNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ShabaNumber)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();
    }
}