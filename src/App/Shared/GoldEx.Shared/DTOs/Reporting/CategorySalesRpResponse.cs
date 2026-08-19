namespace GoldEx.Shared.DTOs.Reporting;

public record CategorySalesRpResponse(
    Guid? CategoryId,
    string CategoryTitle,
    decimal TotalWeight,
    int TotalQuantity,
    decimal TotalAmount,
    decimal TotalProfit,
    decimal TotalWage,
    decimal TotalTax,
    int ItemCount,
    decimal WeightPercentage = 0,
    decimal AmountPercentage = 0);
