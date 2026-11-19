using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Reporting;

public sealed record InvoicePrintDocument(
    long Number,
    InvoiceType Type,
    TradeScale Scale,
    DateOnly Date,
    DateOnly? DueDate,
    InvoicePrintParty Customer,
    string Unit,
    IReadOnlyList<InvoicePrintLine> Items,
    IReadOnlyList<InvoicePrintLine> UsedGold,
    IReadOnlyList<InvoicePrintPayment> Payments,
    decimal Subtotal,
    decimal Discount,
    decimal ExtraCost,
    decimal Total,
    decimal UsedGoldAmount,
    decimal Received,
    decimal Paid,
    decimal Remaining,
    decimal? SecondaryRemaining,
    string? SecondaryUnit,
    InvoicePrintCharges Charges,
    InvoicePrintGoldRate? GoldRate = null);

public sealed record InvoicePrintCharges(
    IReadOnlyList<decimal> ProfitRates,
    IReadOnlyList<decimal> TaxRates,
    decimal Stones);

public sealed record InvoicePrintGoldRate(decimal Amount, GoldUnitType Unit);

public sealed record InvoicePrintParty(string Name, string? Phone, string? Address, string? CustomerCode = null);

public sealed record InvoicePrintLine(
    string Name,
    decimal Quantity,
    decimal Total,
    string? QuantityUnit = null,
    ProductType? ProductType = null,
    string? Barcode = null,
    decimal? Weight = null,
    GoldUnitType? WeightUnit = null,
    decimal? Fineness = null,
    decimal? FinenessDeduction = null,
    string? AssayNumber = null,
    int? MintYear = null,
    InvoicePrintWage? Wage = null);

public sealed record InvoicePrintWage(decimal Amount, WageType Type, string? Unit = null);

public sealed record InvoicePrintPayment(
    DateOnly Date,
    PaymentType Type,
    PaymentSide Side,
    decimal Amount,
    string Unit,
    decimal InvoiceAmount,
    string? Reference,
    DateOnly? CheckDueDate,
    decimal? GoldFineness,
    string? CoinName,
    int? CoinQuantity);
