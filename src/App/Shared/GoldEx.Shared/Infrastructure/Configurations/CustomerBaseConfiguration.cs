using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Shared.Infrastructure.Configurations;

public static class CustomerBaseConfiguration
{
    public static void Configure<T>(EntityTypeBuilder<T> builder) where T : CustomerBase
    {
        builder.ToTable("Customers");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new CustomerId(value));

        builder.Property(x => x.FullName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NationalId)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(25);

        builder.Property(x => x.Address)
            .HasMaxLength(200);
    }
}