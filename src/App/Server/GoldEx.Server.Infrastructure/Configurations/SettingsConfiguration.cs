using GoldEx.Server.Domain.SettingAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal class SettingsConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value,
                value => new SettingsId(value));

        builder.Property(x => x.Address)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PhoneNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.InstitutionName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.TaxPercent)
            .HasPrecision(9, 6);

        builder.Property(x => x.GoldProfitPercent)
            .HasPrecision(9, 6);

        builder.Property(x => x.JewelryProfitPercent)
            .HasPrecision(9, 6);

        builder.Property(x => x.MoltenGoldCommissionPercent)
            .HasPrecision(9, 6);

        builder.Property(x => x.GoldSafetyMarginPercent)
            .HasPrecision(9, 6);

        builder.Property(x => x.UsedGoldFinenessDeductionRate)
            .HasPrecision(9, 6);

        builder.Property(x => x.GramPerMesghal)
            .HasPrecision(36, 10);

        builder.OwnsOne(x => x.BarcodePrintSettings, Configure);
    }

    private void Configure(OwnedNavigationBuilder<Setting, BarcodePrintSettings> builder)
    {
        builder.ToTable("BarcodePrintSettings");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new BarcodePrintSettingsId(id));

        builder.Property(x => x.LabelWidth)
            .IsRequired()
            .HasDefaultValue(80.0);

        builder.Property(x => x.LabelHeight)
            .IsRequired()
            .HasDefaultValue(15.0);

        builder.Property(x => x.TailWidth)
            .IsRequired()
            .HasDefaultValue(30.0);

        builder.OwnsOne(x => x.Margin, margin =>
        {
            margin.Property(m => m.Top).HasColumnName("BarcodeMarginTop").HasDefaultValue(1.0);
            margin.Property(m => m.Right).HasColumnName("BarcodeMarginRight").HasDefaultValue(1.0);
            margin.Property(m => m.Bottom).HasColumnName("BarcodeMarginBottom").HasDefaultValue(1.0);
            margin.Property(m => m.Left).HasColumnName("BarcodeMarginLeft").HasDefaultValue(1.0);
        });

        builder.OwnsOne(x => x.Padding, padding =>
        {
            padding.Property(p => p.Top).HasColumnName("BarcodePaddingTop").HasDefaultValue(1.0);
            padding.Property(p => p.Right).HasColumnName("BarcodePaddingRight").HasDefaultValue(1.0);
            padding.Property(p => p.Bottom).HasColumnName("BarcodePaddingBottom").HasDefaultValue(1.0);
            padding.Property(p => p.Left).HasColumnName("BarcodePaddingLeft").HasDefaultValue(1.0);
        });

        builder.OwnsMany(x => x.PositionItems, items =>
        {
            items.ToTable("BarcodePositionItems");

            items.WithOwner().HasForeignKey("BarcodePrintSettingsId");

            items.Property(i => i.Id)
                .HasConversion(x => x.Value,
                    id => new BarcodePositionItemId(id));

            items.HasKey(nameof(BarcodePositionItem.Id));

            items.Property(i => i.Position)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            items.Property(i => i.ItemType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            items.Property(i => i.Order).HasDefaultValue(0);
            items.Property(i => i.IsVisible).HasDefaultValue(true);
            items.Property(i => i.FontSize).HasDefaultValue(7.0);
            items.Property(i => i.ItemSpacing).HasDefaultValue(0.5);

            items.HasIndex(i => new { i.Position, i.ItemType });

            items.OwnsOne(i => i.BarcodeSettings, bs =>
            {
                bs.Property(s => s.Width)
                    .HasColumnName("BarcodeWidth")
                    .HasDefaultValue(22.0);

                bs.Property(s => s.Height)
                    .HasColumnName("BarcodeHeight")
                    .HasDefaultValue(8.0);

                bs.Property(s => s.DisplayValue)
                    .HasColumnName("BarcodeDisplayValue")
                    .HasDefaultValue(true);

                bs.Property(s => s.FontSize)
                    .HasColumnName("BarcodeFontSize")
                    .HasDefaultValue(7.0);

                bs.Property(s => s.Margin)
                    .HasColumnName("BarcodeMargin")
                    .HasDefaultValue(0.0);

                bs.Property(s => s.BarWidthMultiplier)
                    .HasColumnName("BarWidthMultiplier")
                    .HasDefaultValue(2);
            });
        });

    }
}