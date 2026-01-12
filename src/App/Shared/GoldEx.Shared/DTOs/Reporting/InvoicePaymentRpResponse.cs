using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record InvoicePaymentRpResponse(
    Guid InvoiceId,
    PaymentType PaymentType,
    PaymentSide PaymentSide,
    DateTime PaymentDate,
    string CustomerName,
    decimal InvoiceRemainingPrice,
    string InvoicePriceUnit,
    decimal Amount,
    decimal? ExchangeRate,
    string PriceUnit,
    string? Description);