using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetInvoiceListResponse(
    Guid Id,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    DateTime CreatedAt,
    DateOnly? DueDate,
    InvoiceType InvoiceType,
    string CustomerFullName,
    decimal TotalAmount,
    decimal TotalUnpaidAmount,
    string PriceUnit,
    InvoicePaymentStatus PaymentStatus);