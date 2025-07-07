using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetInvoiceDetailResponse(long InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    string TotalAmount,
    string TotalPaidAmount,
    string TotalDiscountAmount,
    string TotalExtraCostAmount,
    string TotalUnpaidAmount,
    string TotalAmountWithDiscountsAndExtraCosts,
    string DailyGramPrice,
    string TaxPercent,
    string ProfitPercent,
    string TotalUnpaidSecondaryAmount,
    GetCustomerResponse Customer,
    List<GetInvoiceItemReportResponse> InvoiceItems);

public record GetInvoiceItemReportResponse(
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
    GetProductReportResponse Product);

public record GetProductReportResponse(
    string Name,
    string Barcode,
    string Weight,
    string? Wage,
    ProductType ProductType,
    WageType? WageType,
    CaratType CaratType,
    string? ProductCategoryTitle);