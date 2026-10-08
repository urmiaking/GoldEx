using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.SmartTrayAggregate;

public class SmartTray : EntityBase<SmartTrayId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }
    public int TrayNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public Guid? ClerkId { get; private set; }
    public string ClerkName { get; private set; } = string.Empty;
    public SmartTrayStatus Status { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public string? DiscrepancyNotes { get; private set; }
    public Guid? DiscrepancyApprovedBy { get; private set; }

    private readonly List<SmartTrayItem> _items = [];
    public IReadOnlyList<SmartTrayItem> Items => _items.AsReadOnly();

#pragma warning disable CS8618
    private SmartTray() { }
#pragma warning restore CS8618

    public static SmartTray Create(
        int trayNumber,
        string title,
        Guid? clerkId,
        string clerkName,
        StoreId storeId = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(trayNumber, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new SmartTray
        {
            Id = new SmartTrayId(Guid.CreateVersion7()),
            TrayNumber = trayNumber,
            Title = title.Trim(),
            ClerkId = clerkId,
            ClerkName = (clerkName ?? string.Empty).Trim(),
            Status = SmartTrayStatus.Active,
            CreatedAt = DateTime.UtcNow,
            StoreId = storeId
        };
    }

    public SmartTrayItem AddItem(
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
        CoinInstanceId? coinInstanceId)
    {
        if (Status != SmartTrayStatus.Active && Status != SmartTrayStatus.Auditing)
            throw new InvalidOperationException("نمی‌توان به سینی بسته شده یا لغو شده کالای جدید افزود.");

        var normalizedBarcode = barcode.Trim();
        var existing = _items.FirstOrDefault(x => x.Barcode.Equals(normalizedBarcode, StringComparison.OrdinalIgnoreCase));
        if (existing is null && normalizedBarcode.All(char.IsDigit))
        {
            var padded = normalizedBarcode.PadLeft(8, '0');
            var trimmed = normalizedBarcode.TrimStart('0');
            existing = _items.FirstOrDefault(x => x.Barcode.Equals(padded, StringComparison.OrdinalIgnoreCase) || x.Barcode.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        }

        if (existing is not null)
        {
            if (existing.Status == SmartTrayItemStatus.Returned)
            {
                // If previously returned, taking it back to tray
                existing.ResetToInTray();
                return existing;
            }

            throw new InvalidOperationException($"کالایی با بارکد {normalizedBarcode} قبلاً در این سینی ثبت شده است.");
        }

        var item = SmartTrayItem.Create(
            Id,
            itemType,
            normalizedBarcode,
            title,
            weight,
            fineness,
            wage,
            wageType,
            categoryTitle,
            imageUrl,
            productId,
            coinInstanceId,
            StoreId);

        _items.Add(item);
        return item;
    }

    public SmartTrayItem MarkItemReturned(string barcode)
    {
        var normalizedBarcode = barcode.Trim();
        var item = _items.FirstOrDefault(x => x.Barcode.Equals(normalizedBarcode, StringComparison.OrdinalIgnoreCase));
        if (item is null && normalizedBarcode.All(char.IsDigit))
        {
            var padded = normalizedBarcode.PadLeft(8, '0');
            var trimmed = normalizedBarcode.TrimStart('0');
            item = _items.FirstOrDefault(x => x.Barcode.Equals(padded, StringComparison.OrdinalIgnoreCase) || x.Barcode.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        }

        if (item is null)
            throw new InvalidOperationException($"کالایی با بارکد {normalizedBarcode} در این سینی یافت نشد.");

        item.MarkReturned();

        // If in Active mode and an item is returned, optionally set status to Auditing if not completed
        if (Status == SmartTrayStatus.Active && _items.Any(x => x.Status == SmartTrayItemStatus.Returned))
        {
            Status = SmartTrayStatus.Auditing;
        }

        return item;
    }

    public SmartTrayItem MarkItemSold(string barcode, InvoiceId invoiceId)
    {
        var normalizedBarcode = barcode.Trim();
        var item = _items.FirstOrDefault(x => x.Barcode.Equals(normalizedBarcode, StringComparison.OrdinalIgnoreCase));
        if (item is null && normalizedBarcode.All(char.IsDigit))
        {
            var padded = normalizedBarcode.PadLeft(8, '0');
            var trimmed = normalizedBarcode.TrimStart('0');
            item = _items.FirstOrDefault(x => x.Barcode.Equals(padded, StringComparison.OrdinalIgnoreCase) || x.Barcode.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        }

        if (item is null)
            throw new InvalidOperationException($"کالایی با بارکد {normalizedBarcode} در این سینی یافت نشد.");

        item.MarkSold(invoiceId);
        return item;
    }

    public void RemoveItem(string barcode)
    {
        var normalizedBarcode = barcode.Trim();
        var item = _items.FirstOrDefault(x => x.Barcode.Equals(normalizedBarcode, StringComparison.OrdinalIgnoreCase));
        if (item is null && normalizedBarcode.All(char.IsDigit))
        {
            var padded = normalizedBarcode.PadLeft(8, '0');
            var trimmed = normalizedBarcode.TrimStart('0');
            item = _items.FirstOrDefault(x => x.Barcode.Equals(padded, StringComparison.OrdinalIgnoreCase) || x.Barcode.Equals(trimmed, StringComparison.OrdinalIgnoreCase));
        }

        if (item is null)
            throw new InvalidOperationException($"کالایی با بارکد {normalizedBarcode} در این سینی یافت نشد.");

        if (item.Status == SmartTrayItemStatus.Sold)
            throw new InvalidOperationException("کالای فروخته شده را نمی‌توان از سینی حذف کرد.");

        _items.Remove(item);
    }

    public void SetStatus(SmartTrayStatus newStatus)
    {
        Status = newStatus;
    }

    public void CloseTray(Guid closedByUserId)
    {
        var inTrayItems = _items.Where(x => x.Status == SmartTrayItemStatus.InTray).ToList();
        if (inTrayItems.Any())
        {
            throw new InvalidOperationException(
                $"امکان بستن عادی سینی وجود ندارد؛ {inTrayItems.Count} قلم کالا هنوز روی پیش‌خوان است. لطفاً آن‌ها را اسکن و به ویترین بازگردانید یا ثبت مغایرت کنید.");
        }

        Status = SmartTrayStatus.Completed;
        ClosedAt = DateTime.UtcNow;
    }

    public void ReportDiscrepancy(string notes, Guid approvedByUserId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notes);

        var unresolved = _items.Where(x => x.Status == SmartTrayItemStatus.InTray).ToList();
        foreach (var item in unresolved)
        {
            item.MarkMissing();
        }

        Status = SmartTrayStatus.HasDiscrepancy;
        DiscrepancyNotes = notes.Trim();
        DiscrepancyApprovedBy = approvedByUserId;
        ClosedAt = DateTime.UtcNow;
    }

    public void CancelTray()
    {
        if (Status == SmartTrayStatus.Completed)
            throw new InvalidOperationException("سینی تکمیل شده را نمی‌توان لغو کرد.");

        Status = SmartTrayStatus.Cancelled;
        ClosedAt = DateTime.UtcNow;
    }
}
