namespace GoldEx.Shared.DTOs.Reporting;

public record CustomerRemainingBalanceRpResponse(string CustomerName,
    string CustomerCode,
    string CustomerPhoneNumber,
    string PriceUnitTitle,
    decimal PayableAmount,
    decimal ReceivableAmount);