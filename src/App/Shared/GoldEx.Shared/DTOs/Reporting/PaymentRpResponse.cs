using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record PaymentRpResponse(
    Guid InvoiceId,
    long InvoiceNumber,
    InvoiceType InvoiceType,
    PaymentType PaymentType,
    PaymentSide PaymentSide,
    DateTime PaymentDate,
    string CustomerName,
    decimal Amount,
    decimal? ExchangeRate,
    string PriceUnit,
    string? Description);