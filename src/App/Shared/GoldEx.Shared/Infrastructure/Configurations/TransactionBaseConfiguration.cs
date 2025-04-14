using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Shared.Infrastructure.Configurations;

public static class TransactionBaseConfiguration
{
    public static void Configure<TTransaction, TCustomer>(EntityTypeBuilder<TTransaction> builder) 
        where TTransaction : TransactionBase<TCustomer> where TCustomer : CustomerBase
    {
        builder.ToTable("Transactions");
        
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new TransactionId(value));

        builder.Property(x => x.CustomerId)
            .HasConversion(id => id.Value,
                value => new CustomerId(value));

        builder.Property(x => x.DateTime)
            .IsRequired();

        builder.Property(x => x.Description)
            .IsRequired();

        builder.Property(x => x.Number)
            .IsRequired();

        builder.HasOne(x => x.Customer)
            .WithMany()
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}