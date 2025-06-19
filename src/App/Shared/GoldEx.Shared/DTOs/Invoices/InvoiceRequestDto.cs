using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Shared.DTOs.Invoices;

public record InvoiceRequestDto(
    Guid? Id,
    long InvoiceNumber,
    DateTime InvoiceDate,
    DateTime? DueDate,
    CustomerRequestDto Customer,
    List<InvoiceItemDto> InvoiceItems,
    List<InvoiceDiscountDto> InvoiceDiscounts,
    List<InvoicePaymentDto> InvoicePayments,
    List<InvoiceExtraCostsDto> InvoiceExtraCosts);

public record InvoiceExtraCostsDto(decimal Amount, string? Description, Guid PriceUnitId);

public record InvoicePaymentDto(decimal Amount, DateTime PaymentDate, string? ReferenceNumber, string? Note, Guid PaymentMethodId, Guid PriceUnitId);

public record InvoiceDiscountDto(decimal Amount, string? Description, Guid PriceUnitId);

public record InvoiceItemDto(
    Guid? Id,
    decimal GramPrice,
    decimal ProfitPercent,
    decimal TaxPercent,
    decimal? ExchangeRate,
    int Quantity,
    ProductRequestDto Product,
    Guid PriceUnit);