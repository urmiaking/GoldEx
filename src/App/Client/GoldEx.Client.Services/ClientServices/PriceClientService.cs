using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Services.ClientServices;

[ScopedService]
public class PriceClientService(IPriceSyncService syncService, IPriceLocalClientService localService) : IPriceClientService
{
    public async Task<List<GetPriceResponse>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetLatestPricesAsync(cancellationToken);
    }

    public Task<List<GetPriceResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}