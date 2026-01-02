using GoldEx.Shared.DTOs.Reporting;

namespace GoldEx.Shared.Services.Abstractions;

public interface IReportingService
{
    Task<List<LedgerAccountStatementRpResponse>> GetLedgerAccountStatementsAsync(
        LedgerAccountStatementRpRequest request,
        CancellationToken cancellationToken = default);
}