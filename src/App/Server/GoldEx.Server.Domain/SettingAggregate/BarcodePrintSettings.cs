using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.SettingAggregate.ValueObjects;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.SettingAggregate;

public readonly record struct BarcodePrintSettingsId(Guid Value);
public sealed class BarcodePrintSettings : EntityBase<BarcodePrintSettingsId>
{
    private readonly List<BarcodePositionItem> _positionItems = [];

    public int LabelWidth { get; private set; }
    public int LabelHeight { get; private set; }
    public BarcodeMargin Margin { get; private set; } = BarcodeMargin.Default();
    public BarcodePadding Padding { get; private set; } = BarcodePadding.Default();
    public IReadOnlyCollection<BarcodePositionItem> PositionItems => _positionItems.AsReadOnly();

    private BarcodePrintSettings() { }

    private BarcodePrintSettings(int labelWidth, int labelHeight)
    {
        Id = new BarcodePrintSettingsId(Guid.NewGuid());
        LabelWidth = labelWidth;
        LabelHeight = labelHeight;
    }

    public static BarcodePrintSettings CreateDefault()
    {
        var settings = new BarcodePrintSettings(250, 50);

        // تنظیمات پیش‌فرض
        settings.AddPositionItem(BarcodePosition.TopLeft, BarcodePrintableItem.Weight, 0, true, 18, 5);
        settings.AddPositionItem(BarcodePosition.TopRight, BarcodePrintableItem.Barcode, 0, true, 12, 5);
        settings.AddPositionItem(BarcodePosition.BottomLeft, BarcodePrintableItem.Wage, 0, true, 16, 5);
        settings.AddPositionItem(BarcodePosition.BottomRight, BarcodePrintableItem.ProductName, 0, true, 10, 5);

        return settings;
    }

    public void UpdateLabelDimensions(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("ابعاد برچسب باید بزرگتر از صفر باشد");

        LabelWidth = width;
        LabelHeight = height;
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
        int fontSize = 12,
        int itemSpacing = 5,
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
}