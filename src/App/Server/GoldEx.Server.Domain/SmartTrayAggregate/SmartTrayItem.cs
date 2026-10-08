using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.SmartTrayAggregate;

public class SmartTrayItem : EntityBase<SmartTrayItemId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }

    public SmartTrayId SmartTrayId { get; private set; }
    public SmartTray? SmartTray { get; private set; }

    public ItemType ItemType { get; private set; }
    public ProductId? ProductId { get; private set; }
    public CoinInstanceId? CoinInstanceId { get; private set; }

    public string Barcode { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public decimal Weight { get; private set; }
    public decimal Fineness { get; private set; }
    public decimal? Wage { get; private set; }
    public WageType? WageType { get; private set; }
    public string? CategoryTitle { get; private set; }
    public string? ImageUrl { get; private set; }

    public SmartTrayItemStatus Status { get; private set; }
    public DateTime AddedAt { get; private set; }
    public DateTime? ReturnedAt { get; private set; }
    public DateTime? SoldAt { get; private set; }
    public InvoiceId? InvoiceId { get; private set; }

    public TimeSpan Duration => (ReturnedAt ?? SoldAt ?? DateTime.UtcNow) - AddedAt;

#pragma warning disable CS8618
    private SmartTrayItem() { }
#pragma warning restore CS8618

    public static SmartTrayItem Create(
        SmartTrayId smartTrayId,
        ItemType itemType,
        string barcode,
        string title,
        decimal weight,
        decimal fineness,
        decimal? wage,
        WageType? wageType,
        string? categoryTitle,
        string? imageUrl,
        ProductId? productId,
        CoinInstanceId? coinInstanceId,
        StoreId storeId = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(barcode);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new SmartTrayItem
        {
            Id = new SmartTrayItemId(Guid.CreateVersion7()),
            SmartTrayId = smartTrayId,
            ItemType = itemType,
            Barcode = barcode.Trim(),
            Title = title.Trim(),
            Weight = weight,
            Fineness = fineness,
            Wage = wage,
            WageType = wageType,
            CategoryTitle = categoryTitle,
            ImageUrl = imageUrl,
            Status = SmartTrayItemStatus.InTray,
            AddedAt = DateTime.UtcNow,
            ProductId = productId,
            CoinInstanceId = coinInstanceId,
            StoreId = storeId
        };
    }

    public void MarkReturned()
    {
        if (Status == SmartTrayItemStatus.Sold)
            throw new InvalidOperationException("کالایی که به فاکتور منتقل شده و فروخته شده را نمی‌توان به ویترین بازگرداند.");

        Status = SmartTrayItemStatus.Returned;
        ReturnedAt = DateTime.UtcNow;
    }

    public void MarkSold(InvoiceId invoiceId)
    {
        Status = SmartTrayItemStatus.Sold;
        SoldAt = DateTime.UtcNow;
        InvoiceId = invoiceId;
    }

    public void MarkMissing()
    {
        if (Status == SmartTrayItemStatus.Sold || Status == SmartTrayItemStatus.Returned)
            return;

        Status = SmartTrayItemStatus.Missing;
    }

    public void ResetToInTray()
    {
        Status = SmartTrayItemStatus.InTray;
        ReturnedAt = null;
        SoldAt = null;
        InvoiceId = null;
    }
}
