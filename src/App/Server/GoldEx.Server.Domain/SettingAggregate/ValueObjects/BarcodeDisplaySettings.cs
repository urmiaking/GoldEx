using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingAggregate.ValueObjects;

public sealed class BarcodeDisplaySettings : EntityBase
{
    public double Width { get; private set; }
    public double Height { get; private set; }
    public bool DisplayValue { get; private set; }
    public double FontSize { get; private set; }
    public double Margin { get; private set; }
    public int BarWidthMultiplier { get; private set; }

    private BarcodeDisplaySettings() { }

    private BarcodeDisplaySettings(
        double width,
        double height,
        bool displayValue,
        double fontSize,
        double margin,
        int barWidthMultiplier)
    {
        Width = width;
        Height = height;
        DisplayValue = displayValue;
        FontSize = fontSize;
        Margin = margin;
        BarWidthMultiplier = barWidthMultiplier;
    }

    public static BarcodeDisplaySettings Create(
        double width,
        double height,
        bool displayValue = true,
        double fontSize = 7.0,
        double margin = 0.0,
        int barWidthMultiplier = 2)
    {
        if (width is < 5.0 or > 120.0)
            throw new ArgumentException("عرض فیزیکی بارکد باید بین 5 تا 120 میلی‌متر باشد");

        if (height is < 2.0 or > 80.0)
            throw new ArgumentException("ارتفاع فیزیکی بارکد باید بین 2 تا 80 میلی‌متر باشد");

        if (fontSize is < 4.0 or > 30.0)
            throw new ArgumentException("اندازه فونت متن بارکد باید بین 4 تا 30 پوینت باشد");

        if (margin is < 0.0 or > 30.0)
            throw new ArgumentException("حاشیه بارکد باید بین 0 تا 30 میلی‌متر باشد");

        if (barWidthMultiplier is < 1 or > 5)
            throw new ArgumentException("ضریب ضخامت خط بارکد باید بین 1 تا 5 باشد");

        return new BarcodeDisplaySettings(width, height, displayValue, fontSize, margin, barWidthMultiplier);
    }

    public static BarcodeDisplaySettings Default() => new(22.0, 8.0, true, 7.0, 0.0, 2);
}