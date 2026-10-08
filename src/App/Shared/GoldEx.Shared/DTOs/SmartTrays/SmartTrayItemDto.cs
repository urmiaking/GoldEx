using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.SmartTrays;

public class SmartTrayItemDto
{
    public Guid Id { get; set; }
    public Guid SmartTrayId { get; set; }
    public ItemType ItemType { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? CoinInstanceId { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal Fineness { get; set; }
    public decimal? Wage { get; set; }
    public WageType? WageType { get; set; }
    public string? CategoryTitle { get; set; }
    public string? ImageUrl { get; set; }
    public SmartTrayItemStatus Status { get; set; }
    public string StatusTitle { get; set; } = string.Empty;
    public DateTime AddedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public DateTime? SoldAt { get; set; }
    public Guid? InvoiceId { get; set; }
    public double DurationSeconds { get; set; }
}
