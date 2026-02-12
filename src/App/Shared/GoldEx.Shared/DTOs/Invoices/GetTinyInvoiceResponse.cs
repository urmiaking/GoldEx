using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetTinyInvoiceResponse(
    Guid Id,
    decimal Remaining,
    GetPriceUnitTitleResponse PriceUnit,
    decimal? ExchangeRate,
    InvoiceType InvoiceType,
    long InvoiceNumber);