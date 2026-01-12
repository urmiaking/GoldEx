namespace GoldEx.Shared.DTOs.Reporting;

public record SellInvoiceRpResponse(
    Guid Id,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    string CustomerName,
    string PriceUnit,
    decimal TotalPrice,
    decimal RemainingPrice,
    decimal TotalProfit,
    decimal TotalWage,
    decimal TotalTax);