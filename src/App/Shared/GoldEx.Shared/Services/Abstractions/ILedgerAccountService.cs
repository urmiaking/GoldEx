using GoldEx.Shared.DTOs.LedgerAccounts;

namespace GoldEx.Shared.Services.Abstractions;

public interface ILedgerAccountService
{
    Task<List<GetLedgerAccountResponse>> GetListAsync(Guid? customerId, CancellationToken cancellationToken = default);
    Task<GetLedgerAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(LedgerAccountRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, LedgerAccountRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

}