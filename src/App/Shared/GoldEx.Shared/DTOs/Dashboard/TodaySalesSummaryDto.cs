namespace GoldEx.Shared.DTOs.Dashboard;

public class TodaySalesSummaryDto
{
    public string PriceUnit { get; set; } = "تومان";
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public string Subtitle { get; set; } = string.Empty;
}
