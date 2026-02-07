namespace GoldEx.Shared.DTOs.LicensePayments;

public record LicensePaymentRequest(int RequestedMonths, string PaymentReference, string? PaymentDescription);