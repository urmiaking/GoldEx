using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetInvoiceResponse(
    Guid Id,
    long InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    InvoiceType InvoiceType,
    decimal TotalAmount,
    decimal TotalPaidAmount,
    decimal TotalDiscountAmount,
    decimal TotalExtraCostAmount,
    decimal TotalUnpaidAmount,
    decimal TotalUsedProductsAmount,
    decimal TotalAmountWithDiscountsAndExtraCosts,
    decimal? UnpaidAmountExchangeRate,
    decimal? ExchangeRate,
    GetPriceUnitTitleResponse UnpaidPriceUnit,
    GetPriceUnitTitleResponse PriceUnit,
    GetCustomerResponse Customer,
    List<GetInvoiceProductItemResponse> InvoiceProductItems,
    List<GetInvoiceCoinItemResponse> InvoiceCoinItems,
    List<GetInvoiceCurrencyItemResponse> InvoiceCurrencyItems,
    List<GetInvoiceDiscountResponse> InvoiceDiscounts,
    List<GetInvoicePaymentResponse> InvoicePayments,
    List<GetInvoiceExtraCostsResponse> InvoiceExtraCosts,
    List<GetInvoiceUsedProductResponse> InvoiceUsedProducts);

public record GetInvoiceUsedProductResponse(
    Guid Id,
    string Description,
    decimal Weight,
    decimal Fineness,
    decimal GramPrice,
    decimal? ExtraCostsAmount,
    decimal ItemAmount,
    ProductType ProductType,
    GoldUnitType UnitType);

public record GetInvoiceProductItemResponse(
    Guid Id,
    decimal GramPrice,
    decimal ProfitPercent,
    decimal TaxPercent,
    decimal? WageExchangeRate,
    decimal? CostPrice,
    decimal? CostPriceExchangeRate,
    Guid? CostPriceUnitId,
    string? CostPriceUnitTitle,
    decimal? SaleWage,
    WageType? SaleWageType,
    Guid? SaleWagePriceUnitId,
    string SaleWagePriceUnitTitle,
    decimal? SaleWagePriceUnitExchangeRate,
    decimal? StonePriceUnitExchangeRate,
    bool IsInstantProduct,
    int Quantity,
    decimal ItemRawAmount,
    decimal ItemWageAmount,
    decimal ItemProfitAmount,
    decimal ItemTaxAmount,
    decimal ItemFinalAmount,
    decimal TotalAmount,
    decimal? TotalStoneAmount,
    GetProductResponse Product);

public record GetInvoiceCurrencyItemResponse(
    Guid Id,
    decimal UnitPrice,
    decimal Amount,
    decimal TaxPercent,
    decimal ProfitPercent,
    GetPriceUnitTitleResponse Currency);

public record GetInvoiceCoinItemResponse(
    Guid Id,
    decimal UnitPrice,
    int Quantity,
    decimal ProfitPercent,
    GetCoinResponse Coin);

public record GetInvoicePaymentResponse(
    Guid Id,
    decimal Amount,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? Note,
    decimal? ExchangeRate,
    Guid? VoucherId,
    GetFinancialAccountTitleResponse? FinancialAccount,
    GetPriceUnitTitleResponse PriceUnit,
    List<GetFinancialAccountTitleResponse> FinancialAccounts);

public record GetInvoiceExtraCostsResponse(decimal Amount, decimal? ExchangeRate, string? Description, GetPriceUnitTitleResponse PriceUnit);

public record GetInvoiceDiscountResponse(decimal Amount, decimal? ExchangeRate, string? Description, GetPriceUnitTitleResponse PriceUnit);