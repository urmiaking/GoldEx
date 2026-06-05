namespace GoldEx.Shared.DTOs.CheckPayments;

public record AcceptCheckPaymentRequest(
    Guid TargetFinancialAccountId,
    string? Description
);
