using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.DTOs.Invoices;

/// <summary>
/// Represent a DTO to create and update invoice
/// </summary>
/// <param name="Id"></param>
/// <param name="InvoiceNumber"></param>
/// <param name="InvoiceDate"></param>
/// <param name="DueDate"></param>
/// <param name="PriceUnitId"></param>
/// <param name="UnpaidAmountExchangeRate"></param>
/// <param name="UnpaidPriceUnitId"></param>
/// <param name="Customer"></param>
/// <param name="InvoiceItems"></param>
/// <param name="InvoiceDiscounts"></param>
/// <param name="InvoicePayments"></param>
/// <param name="InvoiceExtraCosts"></param>
public record InvoiceRequestDto(
    Guid? Id,
    long InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    Guid PriceUnitId,
    decimal? UnpaidAmountExchangeRate,
    Guid? UnpaidPriceUnitId,
    CustomerRequestDto Customer,
    List<InvoiceItemDto> InvoiceItems,
    List<InvoiceDiscountDto> InvoiceDiscounts,
    List<InvoicePaymentDto> InvoicePayments,
    List<InvoiceExtraCostsDto> InvoiceExtraCosts);

public record InvoicePaymentDto(
    decimal Amount,
    decimal? ExchangeRate,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? Note,
    Guid PaymentMethodId,
    Guid PriceUnitId);

public record InvoiceItemDto(
    Guid? Id,
    decimal GramPrice,
    decimal ProfitPercent,
    decimal TaxPercent,
    decimal? ExchangeRate,
    int Quantity,
    ProductRequestDto Product,
    Guid PriceUnit);

public record InvoiceExtraCostsDto(decimal Amount, decimal? ExchangeRate, string? Description, Guid PriceUnitId);

public record InvoiceDiscountDto(decimal Amount, decimal? ExchangeRate, string? Description, Guid PriceUnitId);