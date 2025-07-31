using GoldEx.Shared.DTOs.FinancialAccounts;

namespace GoldEx.Shared.Services.Abstractions;

public interface IFinancialAccountService
{
    Task<List<GetFinancialAccountResponse>> GetListAsync(CancellationToken cancellationToken = default);
    Task<List<GetFinancialAccountTitleResponse>> GetTitlesAsync(Guid? customerId, Guid? priceUnitId, CancellationToken cancellationToken = default);
    Task<GetFinancialAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsync(FinancialAccountRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, FinancialAccountRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}