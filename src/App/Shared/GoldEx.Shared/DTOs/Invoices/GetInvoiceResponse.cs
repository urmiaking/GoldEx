using GoldEx.Shared.DTOs.CoinInstances;
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
    TradeScale TradeScale,
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
    string Barcode,
    string Description,
    decimal Weight,
    decimal Fineness,
    decimal FinenessDeductionRate,
    decimal GramPrice,
    decimal? ExtraCostsAmount,
    decimal ItemAmount,
    bool IsBroken,
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
    string? SaleWagePriceUnitTitle,
    decimal? PurchaseWage,
    WageType? PurchaseWageType,
    Guid? PurchaseWagePriceUnitId,
    string? PurchaseWagePriceUnitTitle,
    decimal? PurchaseWagePriceUnitExchangeRate,
    decimal? StonePriceUnitExchangeRate,
    bool IsInstantProduct,
    int Quantity,
    decimal TotalWeight,
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
    GetPriceUnitTitleResponse Currency,
    GetFinancialAccountTitleResponse? FinancialAccount);

public record GetInvoiceCoinItemResponse(
    Guid Id,
    decimal UnitPrice,
    int Quantity,
    decimal ProfitPercent,
    bool IsInstant,
    GetCoinInstanceResponse Coin);

public record GetInvoicePaymentResponse(
    Guid Id,
    decimal Amount,
    decimal? GoldFineness,
    PaymentType PaymentType,
    PaymentSide PaymentSide,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? Note,
    decimal? ExchangeRate,
    Guid? VoucherId,
    GetTinyInvoiceResponse? TargetInvoice,
    GetFinancialAccountTitleResponse? FinancialAccount,
    GetCustomerResponse? Endorser,
    GetPriceUnitTitleResponse PriceUnit,
    List<GetFinancialAccountTitleResponse> FinancialAccounts,
    GetCheckPaymentResponse? CheckPayment);

public record GetInvoiceExtraCostsResponse(decimal Amount, decimal? ExchangeRate, string? Description, GetPriceUnitTitleResponse PriceUnit);

public record GetInvoiceDiscountResponse(decimal Amount, decimal? ExchangeRate, string? Description, GetPriceUnitTitleResponse PriceUnit);

public record GetCheckPaymentResponse(
    GetCustomerResponse Issuer,
    GetFinancialAccountResponse IssuerFinancialAccount,
    string? Number,
    string? SayadiCode,
    string? ImageUrl,
    DateTime DueDate,
    CheckPaymentStatus CurrentStatus,
    DateTime LastModifiedAt);