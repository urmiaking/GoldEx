using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Products;

namespace GoldEx.Client.Services.SyncServices;

[ScopedService]
public class ProductSyncService(
    INetworkStatusService networkStatusService,
    IProductLocalClientService productLocalService,
    IProductHttpClientService productHttpService,
    ICheckpointLocalClientService checkpointService
    ) : IProductSyncService
{
    private async Task SyncToServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var pendingProducts = await productLocalService.GetPendingsAsync(checkPointDate, cancellationToken);

        foreach (var product in pendingProducts)
        {
            switch (product.Status)
            {
                case ModifyStatus.Created:
                    {
                        var request = new CreateProductRequest(product.Id,
                            product.Name,
                            product.Barcode,
                            product.Weight,
                            product.Wage,
                            product.WageType,
                            product.ProductType,
                            product.CaratType);

                        await productHttpService.CreateAsync(request, cancellationToken);
                        await productLocalService.SetSyncedAsync(product.Id, cancellationToken);
                        break;
                    }
                case ModifyStatus.Updated:
                    {
                        var request = new UpdateProductRequest(product.Name,
                            product.Barcode,
                            product.Weight,
                            product.Wage,
                            product.WageType,
                            product.ProductType,
                            product.CaratType);

                        await productHttpService.UpdateAsync(product.Id, request, cancellationToken);
                        await productLocalService.SetSyncedAsync(product.Id, cancellationToken);
                        break;
                    }
                case ModifyStatus.Deleted:
                    await productHttpService.DeleteAsync(product.Id, false, cancellationToken);
                    await productLocalService.DeleteAsync(product.Id, true, cancellationToken);
                    break;
                case ModifyStatus.Synced:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await checkpointService.AddCheckPointAsync(nameof(Product), cancellationToken);
    }

    private async Task SyncFromServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var isOnline = await networkStatusService.IsOnlineAsync(cancellationToken);

        if (isOnline)
        {
            var incomingProducts = await productHttpService.GetPendingsAsync(checkPointDate, cancellationToken);

            foreach (var incomingProduct in incomingProducts)
            {
                if (incomingProduct.IsDeleted is true)
                {
                    await productLocalService.DeleteAsync(incomingProduct.Id, true, cancellationToken);
                }
                // Incoming product is available on client and not deleted so update it
                else
                {
                    var localProduct = await productLocalService.GetAsync(incomingProduct.Id, cancellationToken);

                    // Incoming product is not available on client so create it
                    if (localProduct is null)
                    {
                        var createRequest = new CreateProductRequest(incomingProduct.Id,
                            incomingProduct.Name,
                            incomingProduct.Barcode,
                            incomingProduct.Weight,
                            incomingProduct.Wage,
                            incomingProduct.WageType,
                            incomingProduct.ProductType,
                            incomingProduct.CaratType);

                        await productLocalService.CreateAsSyncedAsync(createRequest, cancellationToken);
                    }

                    // Incoming product is available on client so update it
                    else
                    {
                        var updateRequest = new UpdateProductRequest(incomingProduct.Name,
                            incomingProduct.Barcode,
                            incomingProduct.Weight,
                            incomingProduct.Wage,
                            incomingProduct.WageType,
                            incomingProduct.ProductType,
                            incomingProduct.CaratType);

                        await productLocalService.UpdateAsSyncAsync(incomingProduct.Id, updateRequest, cancellationToken);
                    }
                }
            }
        }
    }

    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        var isOnline = await networkStatusService.IsOnlineAsync(cancellationToken);

        if (isOnline)
        {
            var checkPoint = await checkpointService.GetLastCheckPointAsync(nameof(Product), cancellationToken);

            var lastCheckDate = checkPoint?.SyncDate ?? DateTime.MinValue;

            await SyncFromServerAsync(lastCheckDate, cancellationToken);
            await SyncToServerAsync(lastCheckDate, cancellationToken);
        }
    }
}