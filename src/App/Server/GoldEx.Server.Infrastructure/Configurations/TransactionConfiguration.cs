using GoldEx.Server.Domain.TransactionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new TransactionId(value));

        builder.Property(x => x.DateTime)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Number)
            .IsRequired();

        builder.Property(x => x.Credit)
            .HasPrecision(18, 2);

        builder.Property(x => x.CreditRate)
            .HasPrecision(18, 2);

        builder.Property(x => x.Debit)
            .HasPrecision(18, 2);

        builder.Property(x => x.DebitRate)
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}