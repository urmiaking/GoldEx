namespace GoldEx.Server.Infrastructure.Models;

public class CustomerRemainingBalanceModel
{
    public required Guid CustomerId { get; set; } = default!;
    public required string CustomerName { get; set; } = default!;
    public required string CustomerCode { get; set; } = default!;
    public required string CustomerPhoneNumber { get; set; } = default!;
    public required string PriceUnitTitle { get; set; } = default!;
    public required Guid PriceUnitId { get; set; } = default!;
    public required decimal PayableAmount { get; set; }
    public required decimal ReceivableAmount { get; set; }
}