using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class CustomerTransferVoucherConfiguration : IEntityTypeConfiguration<CustomerTransferVoucher>
{
    public void Configure(EntityTypeBuilder<CustomerTransferVoucher> builder)
    {
        builder.ToTable("CustomerTransferVouchers");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new CustomerTransferVoucherId(value));

        builder.Property(x => x.SourceCustomerId)
            .HasConversion(id => id.Value, value => new CustomerId(value));

        builder.Property(x => x.DestinationCustomerId)
            .HasConversion(id => id.Value, value => new CustomerId(value));

        builder.Property(x => x.PriceUnitId)
            .HasConversion(id => id.Value, value => new PriceUnitId(value));

        builder.Property(x => x.SourceInvoiceId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new InvoiceId(value.Value) : null);

        builder.Property(x => x.DestinationInvoiceId)
            .HasConversion(id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new InvoiceId(value.Value) : null);

        builder.Property(x => x.Amount)
            .HasPrecision(36, 10)
            .IsRequired();

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(38, 18);

        builder.Property(x => x.Description)
            .HasMaxLength(250)
            .IsRequired();

        builder.HasIndex(x => new { x.StoreId, x.VoucherNumber })
            .IsUnique();

        builder.HasOne(x => x.SourceCustomer)
            .WithMany()
            .HasForeignKey(x => x.SourceCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationCustomer)
            .WithMany()
            .HasForeignKey(x => x.DestinationCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PriceUnit)
            .WithMany()
            .HasForeignKey(x => x.PriceUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SourceInvoice)
            .WithMany()
            .HasForeignKey(x => x.SourceInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.DestinationInvoice)
            .WithMany()
            .HasForeignKey(x => x.DestinationInvoiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.PriceUnit).AutoInclude();
        builder.Navigation(x => x.SourceCustomer).AutoInclude();
        builder.Navigation(x => x.DestinationCustomer).AutoInclude();
    }
}
