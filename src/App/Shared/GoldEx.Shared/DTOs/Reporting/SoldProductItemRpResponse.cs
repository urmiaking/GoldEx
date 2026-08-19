using System;

namespace GoldEx.Shared.DTOs.Reporting;

public record SoldProductItemRpResponse(
    Guid InvoiceId,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    string? CustomerName,
    Guid ProductId,
    string ProductName,
    string? Barcode,
    Guid? CategoryId,
    string CategoryTitle,
    decimal TotalWeight,
    int Quantity,
    decimal GramPrice,
    decimal WageAmount,
    decimal ProfitAmount,
    decimal TaxAmount,
    decimal FinalAmount,
    string PriceUnit);
