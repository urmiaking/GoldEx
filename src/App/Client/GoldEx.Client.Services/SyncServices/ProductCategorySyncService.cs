using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Client.Offline.Domain.ProductCategoryAggregate;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.ProductCategories;

namespace GoldEx.Client.Services.SyncServices;

[ScopedService]
public class ProductCategorySyncService(
    INetworkStatusService networkStatusService,
    IProductCategoryLocalClientService localService,
    IProductCategoryHttpClientService httpService,
    ICheckpointLocalClientService checkpointService)
    : IProductCategorySyncService
{
    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        var isOnline = await networkStatusService.IsOnlineAsync(cancellationToken);

        if (isOnline)
        {
            var checkPoint = await checkpointService.GetLastCheckPointAsync(nameof(ProductCategory), cancellationToken);

            var lastCheckDate = checkPoint?.SyncDate ?? DateTime.MinValue;

            await SyncFromServerAsync(lastCheckDate, cancellationToken);
            await SyncToServerAsync(lastCheckDate, cancellationToken);
        }
    }

    private async Task SyncToServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var pendingCategories = await localService.GetPendingsAsync(checkPointDate, cancellationToken);

        var shouldAddCheckpoint = true;

        foreach (var pendingCategory in pendingCategories)
        {
            switch (pendingCategory.Status)
            {
                case ModifyStatus.Created:
                    {
                        var request = new CreateCategoryRequest(pendingCategory.Id, pendingCategory.Title);

                        var created = await httpService.CreateAsync(request, cancellationToken);

                        if (created)
                            await localService.SetSyncedAsync(pendingCategory.Id, cancellationToken);
                        else
                            shouldAddCheckpoint = false;

                        break;
                    }
                case ModifyStatus.Updated:
                    {
                        var request = new UpdateCategoryRequest(pendingCategory.Title);

                        var updated = await httpService.UpdateAsync(pendingCategory.Id, request, cancellationToken);
                        if (updated)
                            await localService.SetSyncedAsync(pendingCategory.Id, cancellationToken);
                        else
                            shouldAddCheckpoint = false;

                        break;
                    }
                case ModifyStatus.Deleted:
                    var deleted = await httpService.DeleteAsync(pendingCategory.Id, false, cancellationToken);
                    if (deleted)
                        await localService.DeleteAsync(pendingCategory.Id, true, cancellationToken);
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
            await checkpointService.AddCheckPointAsync(nameof(ProductCategory), cancellationToken);
    }

    private async Task SyncFromServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var incomingCategories = await httpService.GetPendingsAsync(checkPointDate, cancellationToken);

        foreach (var incomingCategory in incomingCategories)
        {
            if (incomingCategory.IsDeleted is true)
            {
                await localService.DeleteAsync(incomingCategory.Id, true, cancellationToken);
            }
            else
            {
                var localCategory = await localService.GetAsync(incomingCategory.Id, cancellationToken);

                // Incoming category is not available on client so create it
                if (localCategory is null)
                {
                    var createRequest = new CreateCategoryRequest(incomingCategory.Id,
                        incomingCategory.Title);

                    await localService.CreateAsSyncedAsync(createRequest, cancellationToken);
                }

                // Incoming category is available on client so update it
                else
                {
                    var updateRequest = new UpdateCategoryRequest(incomingCategory.Title);

                    await localService.UpdateAsSyncAsync(incomingCategory.Id, updateRequest, cancellationToken);
                }
            }
        }
    }
}