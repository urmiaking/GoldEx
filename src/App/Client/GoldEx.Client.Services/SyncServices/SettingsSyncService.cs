using GoldEx.Client.Abstractions.Common;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Client.Offline.Domain.SettingsAggregate;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Settings;

namespace GoldEx.Client.Services.SyncServices;

[ScopedService]
public class SettingsSyncService(
    INetworkStatusService networkStatusService,
    ISettingsHttpClientService httpService,
    ISettingsLocalService localService,
    ICheckpointLocalClientService checkpointService
    ) : ISettingsSyncService
{
    public async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        var isOnline = await networkStatusService.IsOnlineAsync(cancellationToken);

        if (isOnline)
        {
            var checkPoint = await checkpointService.GetLastCheckPointAsync(nameof(Settings), cancellationToken);

            var lastCheckDate = checkPoint?.SyncDate ?? DateTime.MinValue;

            await SyncFromServerAsync(lastCheckDate, cancellationToken);
            await SyncToServerAsync(lastCheckDate, cancellationToken);

            await checkpointService.AddCheckPointAsync(nameof(Settings), cancellationToken);
        }
    }

    private async Task SyncFromServerAsync(DateTime lastCheckDate, CancellationToken cancellationToken = default)
    {
        var incomingUpdate = await httpService.GetUpdateAsync(lastCheckDate, cancellationToken);

        if (incomingUpdate is not null)
        {
            var localSettings = await localService.GetAsync(incomingUpdate.Id, cancellationToken);

            if (localSettings is null)
            {
                var createRequest = new CreateSettingsRequest(incomingUpdate.Id,
                    incomingUpdate.InstitutionName,
                    incomingUpdate.Address,
                    incomingUpdate.PhoneNumber,
                    incomingUpdate.Tax,
                    incomingUpdate.Profit);

                await localService.CreateAsync(createRequest, cancellationToken);
            }
            else
            {
                var updateRequest = new UpdateSettingsRequest(incomingUpdate.InstitutionName,
                    incomingUpdate.Address,
                    incomingUpdate.PhoneNumber,
                    incomingUpdate.Tax,
                    incomingUpdate.Profit);

                await localService.UpdateAsync(incomingUpdate.Id, updateRequest, cancellationToken);
            }
        }
    }

    private async Task SyncToServerAsync(DateTime lastCheckDate, CancellationToken cancellationToken = default)
    {
        var localUpdate = await localService.GetUpdateAsync(lastCheckDate, cancellationToken);

        if (localUpdate is not null)
        {
            var updateRequest = new UpdateSettingsRequest(localUpdate.InstitutionName,
                localUpdate.Address,
                localUpdate.PhoneNumber,
                localUpdate.Tax,
                localUpdate.Profit);

            await httpService.UpdateAsync(localUpdate.Id, updateRequest, cancellationToken);
        }
    }
}