namespace GoldEx.Shared.DTOs.Reporting;

public record LedgerAccountTrialBalanceRpRequest(
    Guid? ParentLedgerId,
    DateTime? FromDate = null,
    DateTime? ToDate = null);