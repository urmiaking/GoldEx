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
        var isOnline = await networkStatusService.IsOnlineAsync(cancellationToken);

        if (isOnline)
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
    }

    private async Task SyncFromServerAsync(DateTime checkPointDate, CancellationToken cancellationToken = default)
    {
        var isOnline = await networkStatusService.IsOnlineAsync(cancellationToken);

        if (isOnline)
        {
            var incomingProducts = await productHttpService.GetPendingsAsync(checkPointDate, cancellationToken);

            foreach (var product in incomingProducts)
            {
                var existingProduct = await productLocalService.GetAsync(product.Id, cancellationToken);

                if (existingProduct is null)
                {
                    var createRequest = new CreateProductRequest(product.Id,
                        product.Name,
                        product.Barcode,
                        product.Weight,
                        product.Wage,
                        product.WageType,
                        product.ProductType,
                        product.CaratType);

                    await productLocalService.CreateAsync(createRequest, ModifyStatus.Synced, cancellationToken);
                }
                else if (product.IsDeleted is null or false)
                {
                    var updateRequest = new UpdateProductRequest(product.Name,
                        product.Barcode,
                        product.Weight,
                        product.Wage,
                        product.WageType,
                        product.ProductType,
                        product.CaratType);

                    await productLocalService.UpdateAsync(product.Id, updateRequest, ModifyStatus.Synced, cancellationToken);
                }
                else
                {
                    await productLocalService.DeleteAsync(product.Id, true, cancellationToken);
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