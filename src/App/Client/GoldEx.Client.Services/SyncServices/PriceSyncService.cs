using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Client.Services.SyncServices;

[ScopedService]
public class PriceSyncService(
    INetworkStatusService networkService,
    ICheckpointLocalClientService checkpointService,
    IPriceHttpClientService httpService,
    IPriceLocalClientService localService) : IPriceSyncService
{
    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        var isOnline = await networkService.IsOnlineAsync(cancellationToken);

        if (isOnline)
        {
            var checkPoint = await checkpointService.GetLastCheckPointAsync(nameof(Price), cancellationToken);

            var lastCheckDate = checkPoint?.SyncDate ?? DateTime.MinValue;

            var pendingPrices = await httpService.GetPendingsAsync(lastCheckDate, cancellationToken);

            foreach (var pendingPrice in pendingPrices)
            {
                var existingPrice = await localService.GetAsync(pendingPrice.Id, cancellationToken);

                if (existingPrice is null)
                {
                    var request = new CreatePriceRequest(pendingPrice.Id,
                        pendingPrice.Title,
                        pendingPrice.Value,
                        pendingPrice.Unit,
                        pendingPrice.Change,
                        pendingPrice.LastUpdate,
                        pendingPrice.IconFileBase64,
                        pendingPrice.Type);

                    await localService.CreateAsync(request, cancellationToken);
                }
                else
                {
                    var request = new UpdatePriceRequest(pendingPrice.Title,
                        pendingPrice.Value,
                        pendingPrice.Unit,
                        pendingPrice.Change,
                        pendingPrice.LastUpdate,
                        pendingPrice.IconFileBase64,
                        pendingPrice.Type);

                    await localService.UpdateAsync(pendingPrice.Id, request, cancellationToken);
                }
            }

            await checkpointService.AddCheckPointAsync(nameof(Price), cancellationToken);
        }
    }
}