using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record PaymentRpResponse(
    Guid InvoiceId,
    long InvoiceNumber,
    InvoiceType InvoiceType,
    PaymentType PaymentType,
    PaymentSide PaymentSide,
    DateTime PaymentDate,
    GetCustomerResponse Customer,
    decimal Amount,
    decimal? ExchangeRate,
    string PriceUnit,
    string? Description);