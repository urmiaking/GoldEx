using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.SettingAggregate.ValueObjects;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.SettingAggregate;

public readonly record struct BarcodePrintSettingsId(Guid Value);
public sealed class BarcodePrintSettings : EntityBase<BarcodePrintSettingsId>
{
    private readonly List<BarcodePositionItem> _positionItems = [];

    public double LabelWidth { get; private set; }
    public double LabelHeight { get; private set; }
    public double TailWidth { get; private set; }
    public BarcodeMargin Margin { get; private set; } = BarcodeMargin.Default();
    public BarcodePadding Padding { get; private set; } = BarcodePadding.Default();
    public IReadOnlyCollection<BarcodePositionItem> PositionItems => _positionItems.AsReadOnly();

    private BarcodePrintSettings() { }

    private BarcodePrintSettings(double labelWidth, double labelHeight, double tailWidth)
    {
        Id = new BarcodePrintSettingsId(Guid.CreateVersion7());
        LabelWidth = labelWidth;
        LabelHeight = labelHeight;
        TailWidth = tailWidth;
    }

    public static BarcodePrintSettings CreateDefault()
    {
        var settings = new BarcodePrintSettings(80.0, 15.0, 30.0);

        // تنظیمات پیش‌فرض
        settings.AddPositionItem(BarcodePosition.TopLeft, BarcodePrintableItem.Weight, 0, true, 8.0, 0.5);
        settings.AddPositionItem(BarcodePosition.TopRight, BarcodePrintableItem.Barcode, 0, true, 8.0, 0.5);
        settings.AddPositionItem(BarcodePosition.BottomLeft, BarcodePrintableItem.Wage, 0, true, 8.0, 0.5);
        settings.AddPositionItem(BarcodePosition.BottomRight, BarcodePrintableItem.ProductName, 0, true, 8.0, 0.5);

        return settings;
    }

    public void UpdateLabelDimensions(double width, double height, double tailWidth)
    {
        if (width <= 0 || height <= 0 || tailWidth < 0)
            throw new ArgumentException("ابعاد برچسب باید بزرگتر از صفر باشد و دم برچسب نمی‌تواند منفی باشد");

        if (tailWidth >= width)
            throw new ArgumentException("عرض دم برچسب نمی‌تواند بزرگتر یا مساوی عرض کل برچسب باشد");

        LabelWidth = width;
        LabelHeight = height;
        TailWidth = tailWidth;
    }

    public void UpdateMargin(BarcodeMargin margin)
    {
        Margin = margin ?? throw new ArgumentNullException(nameof(margin));
    }

    public void UpdatePadding(BarcodePadding padding)
    {
        Padding = padding ?? throw new ArgumentNullException(nameof(padding));
    }

    public void AddPositionItem(
        BarcodePosition position,
        BarcodePrintableItem itemType,
        int order = 0,
        bool isVisible = true,
        double fontSize = 7.0,
        double itemSpacing = 0.5,
        BarcodeDisplaySettings? barcodeDisplaySettings = null)
    {
        // بررسی تکراری نبودن
        if (_positionItems.Any(x => x.Position == position && x.ItemType == itemType))
            throw new InvalidOperationException("این آیتم قبلاً در این موقعیت اضافه شده است");

        var item = BarcodePositionItem.Create(position, itemType, order, isVisible, fontSize, itemSpacing, barcodeDisplaySettings);
        _positionItems.Add(item);
    }

    public void ClearAll()
    {
        _positionItems.Clear();
    }

    public BarcodePrintSettings Clone()
    {
        var clone = new BarcodePrintSettings(LabelWidth, LabelHeight, TailWidth);
        if (Margin != null)
        {
            clone.UpdateMargin(BarcodeMargin.Create(Margin.Top, Margin.Right, Margin.Bottom, Margin.Left));
        }
        if (Padding != null)
        {
            clone.UpdatePadding(BarcodePadding.Create(Padding.Top, Padding.Right, Padding.Bottom, Padding.Left));
        }
        foreach (var item in _positionItems)
        {
            clone.AddPositionItem(
                item.Position,
                item.ItemType,
                item.Order,
                item.IsVisible,
                item.FontSize,
                item.ItemSpacing,
                item.BarcodeSettings != null ? BarcodeDisplaySettings.Create(item.BarcodeSettings.Width, item.BarcodeSettings.Height, item.BarcodeSettings.DisplayValue, item.BarcodeSettings.FontSize, item.BarcodeSettings.Margin, item.BarcodeSettings.BarWidthMultiplier) : null
            );
        }
        return clone;
    }
}