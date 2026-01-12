using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Reporting;

public record PaymentRpRequest(PaymentType? PaymentType,
    PaymentSide? PaymentSide,
    Guid? PriceUnitId,
    Guid? CustomerId, 
    DateTime? FromDate = null,
    DateTime? ToDate = null);