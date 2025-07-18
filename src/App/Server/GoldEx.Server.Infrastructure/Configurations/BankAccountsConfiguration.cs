using GoldEx.Server.Domain.BankAccountAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class BankAccountsConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("BankAccounts");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new BankAccountId(value));

        builder.Property(x => x.AccountHolderName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.BankName)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.BankAccounts)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(x => x.LocalAccount, Configure);
        builder.OwnsOne(x => x.InternationalAccount, Configure);
    }

    private void Configure(OwnedNavigationBuilder<BankAccount, InternationalBankAccount> builder)
    {
        builder.Property(x => x.SwiftBicCode)
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(x => x.IbanNumber)
            .HasMaxLength(34)
            .IsRequired();

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();
    }

    private void Configure(OwnedNavigationBuilder<BankAccount, LocalBankAccount> builder)
    {
        builder.Property(x => x.CardNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ShabaNumber)
            .HasMaxLength(26)
            .IsRequired();

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(20)
            .IsRequired();
    }
}