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
    List<GetInvoiceExtraCostsResponse> InvoiceExtraCosts);

public record GetInvoiceProductItemResponse(
    decimal GramPrice,
    decimal ProfitPercent,
    decimal TaxPercent,
    decimal? ExchangeRate,
    int Quantity,
    decimal ItemRawAmount,
    decimal ItemWageAmount,
    decimal ItemProfitAmount,
    decimal ItemTaxAmount,
    decimal ItemFinalAmount,
    decimal TotalAmount,
    GetProductResponse Product,
    GetPriceUnitTitleResponse PriceUnit);

public record GetInvoiceCurrencyItemResponse(
    decimal UnitPrice,
    decimal Amount,
    decimal TaxPercent,
    decimal ProfitPercent,
    GetPriceUnitTitleResponse Currency);

public record GetInvoiceCoinItemResponse(
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
    GetPriceUnitTitleResponse PriceUnit);

public record GetInvoiceExtraCostsResponse(decimal Amount, decimal? ExchangeRate, string? Description, GetPriceUnitTitleResponse PriceUnit);

public record GetInvoiceDiscountResponse(decimal Amount, decimal? ExchangeRate, string? Description, GetPriceUnitTitleResponse PriceUnit);