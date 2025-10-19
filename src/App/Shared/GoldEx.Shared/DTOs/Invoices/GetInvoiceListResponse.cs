using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetInvoiceListResponse(
    Guid Id,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    DateTime CreatedAt,
    DateOnly? DueDate,
    InvoiceType InvoiceType,
    TradeScale TradeScale,
    string CustomerFullName,
    decimal TotalAmount,
    decimal TotalUnpaidAmount,
    string PriceUnit,
    decimal? TotalUnpaidAmountSecondary,
    string? SecondaryPriceUnit,
    InvoicePaymentStatus PaymentStatus);