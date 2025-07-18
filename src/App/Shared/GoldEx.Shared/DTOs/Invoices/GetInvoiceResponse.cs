﻿using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.DTOs.Invoices;

public record GetInvoiceResponse(
    Guid Id,
    long InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    decimal TotalAmount,
    decimal TotalPaidAmount,
    decimal TotalDiscountAmount,
    decimal TotalExtraCostAmount,
    decimal TotalUnpaidAmount,
    decimal TotalAmountWithDiscountsAndExtraCosts,
    GetPriceUnitTitleResponse PriceUnit,
    GetCustomerResponse Customer,
    List<GetInvoiceItemResponse> InvoiceItems,
    List<GetInvoiceDiscountResponse> InvoiceDiscounts,
    List<GetInvoicePaymentResponse> InvoicePayments,
    List<GetInvoiceExtraCostsResponse> InvoiceExtraCosts);

public record GetInvoiceItemResponse(
    Guid? Id,
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

public record GetInvoicePaymentResponse(
    decimal Amount,
    DateTime PaymentDate,
    string? ReferenceNumber,
    string? Note,
    decimal? ExchangeRate,
    GetPaymentMethodResponse PaymentMethod,
    GetPriceUnitTitleResponse PriceUnit);

public record GetInvoiceExtraCostsResponse(decimal Amount, decimal? ExchangeRate, string? Description, GetPriceUnitTitleResponse PriceUnit);

public record GetInvoiceDiscountResponse(decimal Amount, decimal? ExchangeRate, string? Description, GetPriceUnitTitleResponse PriceUnit);