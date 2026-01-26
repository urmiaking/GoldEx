using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record SellInvoiceRpRequest(
    InvoicePaymentStatus? PaymentStatus = null,
    Guid? PriceUnitId = null,
    Guid? CustomerId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);