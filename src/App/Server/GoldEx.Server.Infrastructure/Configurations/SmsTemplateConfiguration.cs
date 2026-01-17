using GoldEx.Server.Domain.SmsTemplateAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class SmsTemplateConfiguration : IEntityTypeConfiguration<SmsTemplate>
{
    public void Configure(EntityTypeBuilder<SmsTemplate> builder)
    {
        builder.ToTable("SmsTemplates");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new SmsTemplateId(value));

        builder.Property(x => x.Body)
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(x => x.Parameters)
            .HasMaxLength(200);

        builder.Property(x => x.Subject)
            .IsRequired();

        builder.HasIndex(x => x.Subject)
            .IsUnique();
    }
}