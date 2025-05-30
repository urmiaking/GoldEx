using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Domain.SettingsAggregate;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;
using GoldEx.Shared.DTOs.Settings;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class SettingLocalClientService(ISettingsService<Settings> service, IMapper mapper) : ISettingLocalService
{
    public async Task<GetSettingResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(cancellationToken);

        return settings is null ? null : mapper.Map<GetSettingResponse>(settings);
    }

    public async Task<GetSettingResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(new SettingsId(id), cancellationToken);

        return settings is null ? null : mapper.Map<GetSettingResponse>(settings);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateSettingRequest request,
        CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(new SettingsId(id), cancellationToken);

        if (settings is null)
            return false;
        
        settings.SetInstitutionName(request.InstitutionName);
        settings.SetAddress(request.Address);
        settings.SetPhoneNumber(request.PhoneNumber);
        settings.SetTax(request.Tax);
        settings.SetGoldProfit(request.GoldProfit);
        settings.SetJewelryProfit(request.JewelryProfit);

        await service.UpdateAsync(settings, cancellationToken);

        return true;
    }

    public async Task<GetSettingResponse?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        var update = await service.GetUpdateAsync(checkpointDate, cancellationToken);

        return update is null ? null : mapper.Map<GetSettingResponse>(update);
    }

    public async Task CreateAsync(CreateSettingRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(new SettingsId(request.Id), cancellationToken);

        if (settings is not null)
            return;

        await service.CreateAsync(new Settings(new SettingsId(request.Id), request.InstitutionName, request.Address,
            request.PhoneNumber, request.Tax, request.GoldProfit, request.JewelryProfit), cancellationToken);
    }
}