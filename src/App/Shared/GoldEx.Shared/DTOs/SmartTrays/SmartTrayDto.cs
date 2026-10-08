using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.SmartTrays;

public class SmartTrayDto
{
    public Guid Id { get; set; }
    public int TrayNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? ClerkId { get; set; }
    public string ClerkName { get; set; } = string.Empty;
    public SmartTrayStatus Status { get; set; }
    public string StatusTitle { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? DiscrepancyNotes { get; set; }
    public Guid? DiscrepancyApprovedBy { get; set; }
    public List<SmartTrayItemDto> Items { get; set; } = [];

    public decimal TotalWeight => Items.Sum(x => x.Weight);
    public int InTrayCount => Items.Count(x => x.Status == SmartTrayItemStatus.InTray);
    public int ReturnedCount => Items.Count(x => x.Status == SmartTrayItemStatus.Returned);
    public int SoldCount => Items.Count(x => x.Status == SmartTrayItemStatus.Sold);
    public int MissingCount => Items.Count(x => x.Status == SmartTrayItemStatus.Missing);
    public int TotalItemCount => Items.Count;
}
