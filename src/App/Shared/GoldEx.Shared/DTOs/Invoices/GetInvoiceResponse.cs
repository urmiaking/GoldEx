using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetInvoiceResponse(
    Guid Id,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    DateOnly? DueDate,
    string CustomerFullName,
    decimal TotalAmount,
    string AmountUnit,
    InvoicePaymentStatus PaymentStatus);