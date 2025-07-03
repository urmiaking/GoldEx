using GoldEx.Server.Domain.PaymentMethodAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new PaymentMethodId(value));

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Title)
            .IsUnique();
    }
}