namespace GoldEx.Shared.DTOs.PaymentMethods;

public record GetPaymentMethodResponse(Guid Id, string Title, bool IsActive);