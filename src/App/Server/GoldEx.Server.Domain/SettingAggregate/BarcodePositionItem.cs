using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.SettingAggregate.ValueObjects;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.SettingAggregate;

public readonly record struct BarcodePositionItemId(Guid Value);
public sealed class BarcodePositionItem : EntityBase<BarcodePositionItemId>
{
    public BarcodePosition Position { get; private set; }
    public BarcodePrintableItem ItemType { get; private set; }
    public int Order { get; private set; }
    public bool IsVisible { get; private set; }
    public int FontSize { get; private set; }
    public int ItemSpacing { get; private set; }

    public BarcodeDisplaySettings? BarcodeSettings { get; private set; }

    private BarcodePositionItem() { }

    private BarcodePositionItem(
        BarcodePosition position,
        BarcodePrintableItem itemType,
        int order,
        bool isVisible,
        int fontSize,
        int itemSpacing,
        BarcodeDisplaySettings? barcodeSettings = null)
    {
        Id = new BarcodePositionItemId(Guid.NewGuid());
        Position = position;
        ItemType = itemType;
        Order = order;
        IsVisible = isVisible;
        FontSize = fontSize;
        ItemSpacing = itemSpacing;
        BarcodeSettings = itemType == BarcodePrintableItem.Barcode
            ? barcodeSettings ?? BarcodeDisplaySettings.Default()
            : null;
    }

    public static BarcodePositionItem Create(
        BarcodePosition position,
        BarcodePrintableItem itemType,
        int order = 0,
        bool isVisible = true,
        int fontSize = 12,  
        int itemSpacing = 5,
        BarcodeDisplaySettings? barcodeSettings = null)
    {
        if (fontSize <= 0)
            throw new ArgumentException("اندازه فونت باید بزرگتر از صفر باشد");

        if (itemSpacing < 0)
            throw new ArgumentException("فاصله بین آیتم‌ها نمی‌تواند منفی باشد");

        return new BarcodePositionItem(position, itemType, order, isVisible, fontSize, itemSpacing, barcodeSettings);
    }

    public void UpdateVisibility(bool isVisible)
    {
        IsVisible = isVisible;
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("ترتیب نمایش نمی‌تواند منفی باشد");

        Order = order;
    }

    public void UpdateFontSize(int fontSize)
    {
        if (fontSize <= 0)
            throw new ArgumentException("اندازه فونت باید بزرگتر از صفر باشد");

        FontSize = fontSize;
    }

    public void UpdateItemSpacing(int spacing)
    {
        if (spacing < 0)
            throw new ArgumentException("فاصله بین آیتم‌ها نمی‌تواند منفی باشد");

        ItemSpacing = spacing;
    }

    public void UpdateBarcodeSettings(BarcodeDisplaySettings settings)
    {
        if (ItemType != BarcodePrintableItem.Barcode)
            throw new InvalidOperationException("فقط می‌توان تنظیمات بارکد را برای آیتم‌های بارکد تنظیم کرد");

        BarcodeSettings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
}