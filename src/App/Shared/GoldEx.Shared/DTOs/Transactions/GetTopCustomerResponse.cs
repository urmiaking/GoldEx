namespace GoldEx.Shared.DTOs.Transactions;

public record GetTopCustomerResponse(string CustomerName, string PriceUnit, decimal Amount);