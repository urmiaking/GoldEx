using GoldEx.Shared.DTOs.CoinInstances;
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
    TradeScale TradeScale,
    Guid PriceUnitId,
    decimal? UnpaidAmountExchangeRate,
    Guid? UnpaidPriceUnitId,
    decimal? ExchangeRate,
    Guid CustomerId,
    List<InvoiceProductItemDto> InvoiceProductItems,
    List<InvoiceCoinItemDto> InvoiceCoinItems,
    List<InvoiceCurrencyItemDto> InvoiceCurrencyItems,
    List<InvoiceDiscountDto> InvoiceDiscounts,
    List<InvoicePaymentDto> InvoicePayments,
    List<InvoiceExtraCostsDto> InvoiceExtraCosts,
    List<InvoiceUsedProductDto> InvoiceUsedProducts);

public record InvoiceUsedProductDto(
    Guid? Id,
    string Description,
    decimal Weight,
    decimal GramPrice,
    decimal? ExtraCostsAmount,
    decimal Fineness,
    decimal FinenessDeductionRate,
    int Quantity,
    bool IsBroken,
    ProductType ProductType,
    GoldUnitType UnitType);

public record InvoicePaymentDto(
    Guid? Id,
    decimal Amount,
    decimal? ExchangeRate,
    decimal? GoldFineness,
    PaymentType PaymentType,
    PaymentSide PaymentSide,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? Note,
    Guid? FinancialAccountId,
    Guid? VoucherId,
    Guid? TargetInvoiceId,
    Guid? CustomerId,
    Guid PriceUnitId);

public record InvoiceProductItemDto(
    Guid? Id,
    decimal GramPrice,
    decimal ProfitPercent,
    decimal TaxPercent,
    decimal? CostPrice,
    decimal? CostPriceExchangeRate,
    decimal? WagePriceUnitExchangeRate,
    decimal? StonePriceUnitExchangeRate,
    Guid? CostPriceUnitId,
    bool IsInstantProduct,
    int Quantity,
    decimal TotalWeight,
    decimal? PurchaseWage,
    WageType? PurchaseWageType,
    ProductRequestDto Product);

public record InvoiceCoinItemDto(
    Guid? Id,
    decimal UnitPrice,
    int Quantity,
    decimal ProfitPercent,
    bool IsInstant,
    CoinInstanceRequestDto CoinInstance);

public record InvoiceCurrencyItemDto(
    Guid? Id,
    decimal UnitPrice,
    decimal Amount,
    decimal ProfitPercent,
    decimal TaxPercent,
    Guid CurrencyId,
    Guid FinancialAccountId);

public record InvoiceExtraCostsDto(decimal Amount, decimal? ExchangeRate, string? Description, Guid PriceUnitId);

public record InvoiceDiscountDto(decimal Amount, decimal? ExchangeRate, string? Description, Guid PriceUnitId);