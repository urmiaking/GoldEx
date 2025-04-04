﻿using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Abstractions.SyncServices;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Services.ClientServices;

[ScopedService]
public class SettingsClientService(
    ISettingsLocalService localService,
    ISettingsSyncService syncService)
    : ISettingsClientService
{
    public async Task<GetSettingsResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(cancellationToken);
    }

    public async Task<GetSettingsResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await syncService.SynchronizeAsync(cancellationToken);

        return await localService.GetAsync(id, cancellationToken);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        await localService.UpdateAsync(id, request, cancellationToken);
            
        await syncService.SynchronizeAsync(cancellationToken);

        return true;
    }

    public Task<GetSettingsResponse?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}