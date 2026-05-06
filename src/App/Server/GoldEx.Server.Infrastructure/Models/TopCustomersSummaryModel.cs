namespace GoldEx.Server.Infrastructure.Models;

public class TopCustomersSummaryModel
{
    public required string CustomerName { get; set; }
    public required string PriceUnit { get; set; }
    public decimal Amount { get; set; }
}