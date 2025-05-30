using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Client.Offline.Domain.TransactionAggregate;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Client.Services.SyncServices;

[ScopedService]
public class TransactionSyncService(INetworkStatusService networkStatusService,
    ICheckpointLocalClientService checkpointService,
    ITransactionLocalClientService localService,
    ITransactionHttpService httpService
    ) : ITransactionSyncService
{
    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        var isOnline = await networkStatusService.IsOnlineAsync(cancellationToken);

        if (isOnline)
        {
            var checkPoint = await checkpointService.GetLastCheckPointAsync(nameof(Transaction), cancellationToken);

            var lastCheckDate = checkPoint?.SyncDate ?? DateTime.MinValue;

            await SyncFromServerAsync(lastCheckDate, cancellationToken);
            await SyncToServerAsync(lastCheckDate, cancellationToken);
        }
    }

    private async Task SyncToServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var pendingTransactions = await localService.GetPendingsAsync(checkPointDate, cancellationToken);

        var shouldAddCheckpoint = true;

        foreach (var transaction in pendingTransactions)
        {
            switch (transaction.Status)
            {
                case ModifyStatus.Created:
                    {
                        var request = new CreateTransactionRequest(transaction.Id,
                            transaction.Number,
                            transaction.Description,
                            transaction.DateTime,
                            transaction.Credit,
                            transaction.CreditUnit,
                            transaction.CreditRate,
                            transaction.Debit,
                            transaction.DebitUnit,
                            transaction.DebitRate,
                            transaction.CustomerId);

                        var created = await httpService.CreateAsync(request, cancellationToken);
                        if (created)
                            await localService.SetSyncedAsync(transaction.Id, cancellationToken);
                        else
                            shouldAddCheckpoint = false;

                        break;
                    }
                case ModifyStatus.Updated:
                    {
                        var request = new UpdateTransactionRequest(transaction.Number,
                            transaction.Description,
                            transaction.DateTime,
                            transaction.Credit,
                            transaction.CreditUnit,
                            transaction.CreditRate,
                            transaction.Debit,
                            transaction.DebitUnit,
                            transaction.DebitRate,
                            transaction.CustomerId);

                        var updated = await httpService.UpdateAsync(transaction.Id, request, cancellationToken);
                        if (updated)
                            await localService.SetSyncedAsync(transaction.Id, cancellationToken);
                        else
                            shouldAddCheckpoint = false;

                        break;
                    }
                case ModifyStatus.Deleted:
                    var deleted = await httpService.DeleteAsync(transaction.Id, cancellationToken);
                    if (deleted)
                        await localService.DeleteAsync(transaction.Id, cancellationToken);
                    else
                        shouldAddCheckpoint = false;

                    break;
                case ModifyStatus.Synced:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        if (shouldAddCheckpoint)
            await checkpointService.AddCheckPointAsync(nameof(Transaction), cancellationToken);
    }

    private async Task SyncFromServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var incomingTransactions = await httpService.GetPendingsAsync(checkPointDate, cancellationToken);

        foreach (var incomingTransaction in incomingTransactions)
        {
            if (incomingTransaction.IsDeleted is true)
            {
                await localService.DeleteAsync(incomingTransaction.Id, cancellationToken);
            }
            else
            {
                var localTransaction = await localService.GetAsync(incomingTransaction.Id, cancellationToken);

                // Incoming transaction is not available on client so create it
                if (localTransaction is null)
                {
                    var createRequest = new CreateTransactionRequest(incomingTransaction.Id,
                        incomingTransaction.Number,
                        incomingTransaction.Description,
                        incomingTransaction.DateTime,
                        incomingTransaction.Credit,
                        incomingTransaction.CreditUnit,
                        incomingTransaction.CreditRate,
                        incomingTransaction.Debit,
                        incomingTransaction.DebitUnit,
                        incomingTransaction.DebitRate,
                        incomingTransaction.CustomerId);

                    await localService.CreateAsSyncedAsync(createRequest, cancellationToken);
                }

                // Incoming transaction is available on client so update it
                else
                {
                    var updateRequest = new UpdateTransactionRequest(incomingTransaction.Number,
                        incomingTransaction.Description,
                        incomingTransaction.DateTime,
                        incomingTransaction.Credit,
                        incomingTransaction.CreditUnit,
                        incomingTransaction.CreditRate,
                        incomingTransaction.Debit,
                        incomingTransaction.DebitUnit,
                        incomingTransaction.DebitRate,
                        incomingTransaction.CustomerId);

                    await localService.UpdateAsSyncAsync(incomingTransaction.Id, updateRequest, cancellationToken);
                }
            }
        }
    }
}