using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface ILedgerAccountService
{
    Task<List<GetLedgerAccountResponse>> GetListAsync(Guid? customerId, CancellationToken cancellationToken = default);
    Task<List<GetLedgerAccountResponse>> GetTitlesAsync(FinancialAccountType? financialAccountType, 
        CancellationToken cancellationToken = default);
    Task<List<GetLedgerAccountResponse>> GetActiveLedgerAccountsAsync(CancellationToken cancellationToken = default);
    Task<List<GetLedgerAccountResponse>> GetParentLedgerAccountsAsync(CancellationToken cancellationToken = default);
    Task<GetLedgerAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(LedgerAccountRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, LedgerAccountRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}