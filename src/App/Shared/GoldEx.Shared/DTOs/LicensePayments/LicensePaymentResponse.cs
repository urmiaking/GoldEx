using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.LicensePayments;

public record LicensePaymentResponse(
    Guid Id,
    LicensePlan CurrentPlan,
    int RequestedMonths,
    string PaymentReference,
    string? PaymentDescription,
    LicensePaymentStatus Status,
    DateTime CreatedAt);