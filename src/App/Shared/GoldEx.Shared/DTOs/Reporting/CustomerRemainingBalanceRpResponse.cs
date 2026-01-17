namespace GoldEx.Shared.DTOs.Reporting;

public record CustomerRemainingBalanceRpResponse(Guid CustomerId,
    string CustomerName,
    string CustomerCode,
    string CustomerPhoneNumber,
    string PriceUnitTitle,
    Guid PriceUnitId,
    decimal PayableAmount,
    decimal ReceivableAmount);