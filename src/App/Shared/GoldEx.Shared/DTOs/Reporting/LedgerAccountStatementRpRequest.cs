namespace GoldEx.Shared.DTOs.Reporting;

public record LedgerAccountStatementRpRequest(
    Guid LedgerAccountId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? PriceUnitId = null);