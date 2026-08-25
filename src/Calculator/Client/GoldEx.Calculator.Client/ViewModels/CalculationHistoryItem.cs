namespace GoldEx.Calculator.Client.ViewModels;

public sealed class CalculationHistoryItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..8];
    public required string Title { get; init; }
    public required string Category { get; init; } // e.g. "طلا و جواهر", "تعویض طلا", "حباب سکه", "عیارسنجی", "سرمایه‌گذاری", "ارز به طلا"
    public required string SummaryText { get; init; }
    public required string ResultValue { get; init; }
    public string? Unit { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.Now;
    public Dictionary<string, string> Details { get; init; } = [];
}
