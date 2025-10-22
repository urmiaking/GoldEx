using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetInvoiceDetailResponse(
    long InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    InvoiceType InvoiceType,
    TradeScale TradeScale,
    string TotalAmount,
    string TotalPaidAmount,
    string? TotalDiscountAmount,
    string? TotalExtraCostAmount,
    string TotalUnpaidAmount,
    string TotalPayableAmount,
    string? TotalUsedProductsAmount,
    string? TotalStoneAmount,
    string? DailyGramPrice,
    string TaxPercent,
    string ProfitPercent,
    string TotalUnpaidSecondaryAmount,
    GoldUnitType GoldUnitType,
    GetCustomerResponse Customer,
    List<GetInvoiceProductItemReportResponse> InvoiceProductItems,
    List<GetInvoiceCoinItemReportResponse> InvoiceCoinItems,
    List<GetInvoiceCurrencyItemReportResponse> InvoiceCurrencyItems,
    List<GetInvoiceUsedProductReportResponse> InvoiceUsedProductItems,
    List<GetInvoicePaymentReportResponse> InvoicePayments, 
    string? PreviousRemaining,
    string? AfterRemaining);

public record GetInvoicePaymentReportResponse(
    string FinalAmount,
    PaymentType PaymentType,
    DateTime PaymentDate,
    string Description);

public record GetInvoiceProductItemReportResponse(
    string GramPrice,
    string ProfitPercent,
    string TaxPercent,
    string? ExchangeRate,
    int Quantity,
    string ItemRawAmount,
    string ItemWageAmount,
    string ItemProfitAmount,
    string ItemTaxAmount,
    string ItemFinalAmount,
    string TotalAmount,
    string SaleWage,
    string TotalWeight,
    WageType? SaleWageType,
    GetProductReportResponse Product);

public record GetProductReportResponse(
    string Name,
    string Barcode,
    string Weight,
    string? Wage,
    ProductType ProductType,
    WageType? WageType,
    GoldUnitType GoldUnitType,
    decimal Fineness,
    string? ProductCategoryTitle);

public record GetInvoiceCoinItemReportResponse(
    string CoinTitle,
    string TotalPrice,
    int Quantity);

public record GetInvoiceCurrencyItemReportResponse(
    string CurrencyTitle,
    string TotalPrice,
    string Amount);

public record GetInvoiceUsedProductReportResponse(
    string Title,
    string Weight,
    string? ExtraCosts,
    string TotalPrice,
    decimal Fineness,
    int Quantity,
    GoldUnitType GoldUnitType,
    ProductType ProductType);