using GoldEx.Server.Domain.ReportLayoutAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

public class ReportLayoutConfiguration : IEntityTypeConfiguration<ReportLayout>
{
    public void Configure(EntityTypeBuilder<ReportLayout> builder)
    {
        builder.ToTable("ReportLayouts");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new ReportLayoutId(value));

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.Property(x => x.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LayoutData)
            .IsRequired();
    }
}