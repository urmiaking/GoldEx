using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public sealed record CustomerComprehensiveRpResponse(
    Guid CustomerId,
    List<CustomerComprehensiveInvoice> Invoices,
    List<CustomerComprehensivePayment> Payments,
    List<CustomerTransactionRpResponse> Transactions,
    List<CustomerComprehensiveBalance> Balances);

public sealed record CustomerComprehensiveInvoice(
    Guid Id,
    long InvoiceNumber,
    DateOnly InvoiceDate,
    InvoiceType InvoiceType,
    TradeScale TradeScale,
    string PriceUnitTitle,
    decimal TotalAmount,
    decimal DiscountAmount,
    decimal ExtraCostAmount,
    List<CustomerComprehensiveItem> Items,
    int ItemCount);

public sealed record CustomerComprehensiveItem(
    string Category,
    string Description,
    decimal Quantity,
    decimal? Weight,
    decimal? Fineness,
    decimal Amount);

public sealed record CustomerComprehensivePayment(
    Guid Id,
    Guid InvoiceId,
    long InvoiceNumber,
    DateTime PaymentDate,
    PaymentType PaymentType,
    PaymentSide PaymentSide,
    decimal Amount,
    string PriceUnitTitle,
    string? ReferenceNumber,
    string? Note,
    Guid? PaymentVoucherId,
    Guid? CustomerTransferVoucherId);

public sealed record CustomerComprehensiveBalance(string PriceUnitTitle, decimal Amount);
