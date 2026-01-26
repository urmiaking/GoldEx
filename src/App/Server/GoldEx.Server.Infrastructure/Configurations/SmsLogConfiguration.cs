using GoldEx.Server.Domain.SmsLogAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class SmsLogConfiguration : IEntityTypeConfiguration<SmsLog>
{
    public void Configure(EntityTypeBuilder<SmsLog> builder)
    {
        builder.ToTable("SmsLogs");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new SmsLogId(value));

        builder.Property(x => x.Message)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(x => x.Receiver)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(x => x.Receiver);
    }
}