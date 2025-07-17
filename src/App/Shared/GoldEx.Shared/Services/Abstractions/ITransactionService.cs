using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Shared.Services.Abstractions;

public interface ITransactionService
{
    Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, TransactionFilter transactionFilter,
        Guid? customerId, CancellationToken cancellationToken = default);
    Task<GetTransactionResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetTransactionResponse> GetAsync(int number, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<GetTransactionNumberResponse> GetLastNumberAsync(CancellationToken cancellationToken = default);
}