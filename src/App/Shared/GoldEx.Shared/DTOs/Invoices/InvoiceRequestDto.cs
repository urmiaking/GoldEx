using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

/// <summary>
/// Represent a DTO to create and update invoice
/// </summary>
/// <param name="Id"></param>
/// <param name="InvoiceNumber"></param>
/// <param name="InvoiceDate"></param>
/// <param name="DueDate"></param>
/// <param name="InvoiceType"></param>
/// <param name="PriceUnitId"></param>
/// <param name="UnpaidAmountExchangeRate"></param>
/// <param name="UnpaidPriceUnitId"></param>
/// <param name="ExchangeRate"></param>
/// <param name="Customer"></param>
/// <param name="InvoiceProductItems"></param>
/// <param name="InvoiceCoinItems"></param>
/// <param name="InvoiceCurrencyItems"></param>
/// <param name="InvoiceDiscounts"></param>
/// <param name="InvoicePayments"></param>
/// <param name="InvoiceExtraCosts"></param>
public record InvoiceRequestDto(
    Guid? Id,
    long InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    InvoiceType InvoiceType,
    Guid PriceUnitId,
    decimal? UnpaidAmountExchangeRate,
    Guid? UnpaidPriceUnitId,
    decimal? ExchangeRate,
    CustomerRequestDto Customer,
    List<InvoiceProductItemDto> InvoiceProductItems,
    List<InvoiceCoinItemDto> InvoiceCoinItems,
    List<InvoiceCurrencyItemDto> InvoiceCurrencyItems,
    List<InvoiceDiscountDto> InvoiceDiscounts,
    List<InvoicePaymentDto> InvoicePayments,
    List<InvoiceExtraCostsDto> InvoiceExtraCosts);

public record InvoicePaymentDto(
    Guid? Id,
    decimal Amount,
    decimal? ExchangeRate,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? Note,
    Guid? FinancialAccountId,
    Guid? VoucherId,
    Guid PriceUnitId);

public record InvoiceProductItemDto(
    decimal GramPrice,
    decimal ProfitPercent,
    decimal TaxPercent,
    decimal? ExchangeRate,
    int Quantity,
    ProductRequestDto Product,
    Guid PriceUnit);

public record InvoiceCoinItemDto(
    decimal UnitPrice,
    int Quantity,
    decimal ProfitPercent,
    Guid CoinId);

public record InvoiceCurrencyItemDto(
    decimal UnitPrice,
    decimal Amount,
    decimal ProfitPercent,
    decimal TaxPercent,
    Guid CurrencyId);

public record InvoiceExtraCostsDto(decimal Amount, decimal? ExchangeRate, string? Description, Guid PriceUnitId);

public record InvoiceDiscountDto(decimal Amount, decimal? ExchangeRate, string? Description, Guid PriceUnitId);