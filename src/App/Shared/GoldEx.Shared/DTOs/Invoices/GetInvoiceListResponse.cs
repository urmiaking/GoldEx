using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetInvoiceListResponse(
    Guid Id,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    string CustomerFullName,
    decimal TotalAmount,
    string AmountUnit,
    InvoicePaymentStatus PaymentStatus);