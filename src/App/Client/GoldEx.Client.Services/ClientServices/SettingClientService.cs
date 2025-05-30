using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Services.ClientServices;

[ScopedService]
public class SettingClientService(
    ISettingLocalService localService,
    ISettingsSyncService syncService)
    : ISettingService
{
    public async Task<GetSettingResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(cancellationToken);
    }

    public async Task<GetSettingResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(id, cancellationToken);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        await localService.UpdateAsync(request, cancellationToken);
            
        await syncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public Task<GetSettingResponse?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}