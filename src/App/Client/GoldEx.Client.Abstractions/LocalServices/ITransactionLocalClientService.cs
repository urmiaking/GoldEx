using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface ITransactionLocalClientService : ITransactionService
{
    Task SetSyncedAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateAsSyncedAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsSyncAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default);
}