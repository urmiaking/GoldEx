using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Shared.DTOs.Reporting;

public record PurchaseInvoiceRpResponse(
    Guid Id,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    GetCustomerResponse Customer,
    string PriceUnit,
    decimal TotalPrice,
    decimal RemainingPrice);