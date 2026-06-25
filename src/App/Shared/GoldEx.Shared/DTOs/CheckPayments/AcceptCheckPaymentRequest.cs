using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.CheckPayments;

public record AcceptCheckPaymentRequest(
    Guid TargetFinancialAccountId,
    string? Description,
    decimal? CashingExchangeRate = null,
    CheckCashingAdjustmentMode? AdjustmentMode = null,
    bool SettleDifference = false,
    Guid? SettlementFinancialAccountId = null
);
