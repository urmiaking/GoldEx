using GoldEx.Server.Domain.BarcodeInquiryAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class BarcodeInquiryConfiguration : IEntityTypeConfiguration<BarcodeInquiry>
{
    public void Configure(EntityTypeBuilder<BarcodeInquiry> builder)
    {
        builder.ToTable("BarcodeInquiries");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new BarcodeInquiryId(id));

        builder.Property(x => x.Barcode)
            .IsRequired()
           .HasMaxLength(50);

        builder.Property(x => x.InquiryDate)
            .IsRequired();

        builder.HasIndex(x => x.Barcode);
    }
}