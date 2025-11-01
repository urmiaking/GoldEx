using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.SettingAggregate.ValueObjects;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.SettingAggregate;

public readonly record struct BarcodePrintSettingsId(Guid Value);
public sealed class BarcodePrintSettings : EntityBase<BarcodePrintSettingsId>
{
    private readonly List<BarcodePositionItem> _positionItems = new();

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
        var settings = new BarcodePrintSettings(300, 150);

        // تنظیمات پیش‌فرض
        settings.AddPositionItem(BarcodePosition.TopLeft, BarcodePrintableItem.Barcode, 0, true, 14, 5);
        settings.AddPositionItem(BarcodePosition.TopRight, BarcodePrintableItem.ProductName, 0, true, 12, 3);
        settings.AddPositionItem(BarcodePosition.BottomLeft, BarcodePrintableItem.Weight, 0, true, 11, 3);
        settings.AddPositionItem(BarcodePosition.BottomRight, BarcodePrintableItem.Wage, 0, true, 11, 3);

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

    public void RemovePositionItem(BarcodePositionItemId itemId)
    {
        var item = _positionItems.FirstOrDefault(x => x.Id == itemId);
        if (item != null)
            _positionItems.Remove(item);
    }

    public void ClearPosition(BarcodePosition position)
    {
        _positionItems.RemoveAll(x => x.Position == position);
    }

    public void ClearAll()
    {
        _positionItems.Clear();
    }

    public IEnumerable<BarcodePositionItem> GetItemsForPosition(BarcodePosition position)
    {
        return _positionItems
            .Where(x => x.Position == position && x.IsVisible)
            .OrderBy(x => x.Order);
    }
}