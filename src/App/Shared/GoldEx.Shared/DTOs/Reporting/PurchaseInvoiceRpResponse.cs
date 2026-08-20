namespace GoldEx.Shared.DTOs.Reporting;

public record PurchaseInvoiceRpResponse(
    Guid Id,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    string CustomerName,
    string PriceUnit,
    decimal TotalPrice,
    decimal RemainingPrice,
    decimal TotalWeightEquivalent = 0,
    decimal RemainingWeightEquivalent = 0);