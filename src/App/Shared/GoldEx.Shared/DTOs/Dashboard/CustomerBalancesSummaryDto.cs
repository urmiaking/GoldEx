namespace GoldEx.Shared.DTOs.Dashboard;

public class CustomerBalancesSummaryDto
{
    public List<PriceUnitSummaryDto> Receivables { get; set; } = [];
    public List<PriceUnitSummaryDto> Payables { get; set; } = [];
}

public class PriceUnitSummaryDto
{
    public string PriceUnit { get; set; } = "تومان";
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public string Subtitle { get; set; } = string.Empty;
}
