using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SettingAggregate.ValueObjects;

public sealed class BarcodeDisplaySettings : EntityBase
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool DisplayValue { get; private set; }
    public int FontSize { get; private set; }
    public int Margin { get; private set; }

    private BarcodeDisplaySettings() { }

    private BarcodeDisplaySettings(
        int width,
        int height,
        bool displayValue,
        int fontSize,
        int margin)
    {
        Width = width;
        Height = height;
        DisplayValue = displayValue;
        FontSize = fontSize;
        Margin = margin;
    }

    public static BarcodeDisplaySettings Create(
        int width,
        int height,
        bool displayValue = true,
        int fontSize = 14,
        int margin = 0)
    {
        if (width is < 1 or > 5)
            throw new ArgumentException("عرض خط بارکد باید بین 1 تا 5 باشد");

        if (height is < 10 or > 150)
            throw new ArgumentException("ارتفاع بارکد باید بین 10 تا 150 پیکسل باشد");

        if (fontSize is < 8 or > 24)
            throw new ArgumentException("اندازه فونت بارکد باید بین 8 تا 24 باشد");

        if (margin is < 0 or > 50)
            throw new ArgumentException("حاشیه بارکد باید بین 0 تا 50 باشد");

        return new BarcodeDisplaySettings(width, height, displayValue, fontSize, margin);
    }

    public static BarcodeDisplaySettings Default() => new(2, 50, true, 14, 0);
}