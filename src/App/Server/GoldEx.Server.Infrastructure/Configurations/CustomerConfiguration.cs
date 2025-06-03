using GoldEx.Server.Domain.CustomerAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
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

        builder.Property(x => x.CreditLimit)
            .HasPrecision(36, 10);
    }
}
