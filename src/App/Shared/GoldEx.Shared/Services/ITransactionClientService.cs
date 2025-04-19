using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Shared.Services;

public interface ITransactionClientService
{
    Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default);
    Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, Guid customerId, CancellationToken cancellationToken = default);
    Task<GetTransactionResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetTransactionResponse?> GetAsync(int number, CancellationToken cancellationToken = default);
    Task<bool> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default);
    Task<List<GetPendingTransactionResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
    Task<GetTransactionNumberResponse> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default);
}