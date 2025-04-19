using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Services.ClientServices;

[ScopedService]
public class TransactionClientService(
    ITransactionLocalClientService localService,
    ITransactionSyncService transactionSyncService,
    ICustomerSyncService customerSyncService
    ) : ITransactionClientService
{
    public async Task<PagedList<GetTransactionResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        await customerSyncService.SynchronizeAsync(cancellationToken);
        await transactionSyncService.SynchronizeAsync(cancellationToken);

        return await localService.GetListAsync(filter, cancellationToken);
    }

    public async Task<GetTransactionResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await customerSyncService.SynchronizeAsync(cancellationToken);
        await transactionSyncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(id, cancellationToken);
    }

    public async Task<GetTransactionResponse?> GetAsync(int number, CancellationToken cancellationToken = default)
    {
        await customerSyncService.SynchronizeAsync(cancellationToken);
        await transactionSyncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(number, cancellationToken);
    }

    public async Task<bool> CreateAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await localService.CreateAsync(request, cancellationToken);

        await customerSyncService.SynchronizeAsync(cancellationToken);
        await transactionSyncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        await localService.UpdateAsync(id, request, cancellationToken);

        await customerSyncService.SynchronizeAsync(cancellationToken);
        await transactionSyncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        await localService.DeleteAsync(id, false, cancellationToken);

        await customerSyncService.SynchronizeAsync(cancellationToken);
        await transactionSyncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public Task<List<GetPendingTransactionResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<GetTransactionNumberResponse> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default)
    {
        await customerSyncService.SynchronizeAsync(cancellationToken);
        await transactionSyncService.SynchronizeAsync(cancellationToken);

        return await localService.GetLatestTransactionNumberAsync(cancellationToken);
    }
}