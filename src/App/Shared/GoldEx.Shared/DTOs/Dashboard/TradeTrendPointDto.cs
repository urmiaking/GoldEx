namespace GoldEx.Shared.DTOs.Dashboard;

public class TradeTrendPointDto
{
    public DateOnly Date { get; set; }
    public decimal SellWeight { get; set; }
    public decimal PurchaseWeight { get; set; }
}
