using GoldEx.Client.Abstractions.LocalServices;
using GoldEx.Client.Offline.Domain.SettingsAggregate;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;
using GoldEx.Shared.DTOs.Settings;
using MapsterMapper;

namespace GoldEx.Client.Services.LocalServices;

[ScopedService]
public class SettingsLocalClientService(ISettingsService<Settings> service, IMapper mapper) : ISettingsLocalService
{
    public async Task<GetSettingsResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(cancellationToken);

        return settings is null ? null : mapper.Map<GetSettingsResponse>(settings);
    }

    public async Task<GetSettingsResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(new SettingsId(id), cancellationToken);

        return settings is null ? null : mapper.Map<GetSettingsResponse>(settings);
    }

    public async Task UpdateAsync(Guid id, UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(new SettingsId(id), cancellationToken);

        if (settings is null)
            return;
        
        settings.SetInstitutionName(request.InstitutionName);
        settings.SetAddress(request.Address);
        settings.SetPhoneNumber(request.PhoneNumber);
        settings.SetTax(request.Tax);
        settings.SetProfit(request.Profit);

        await service.UpdateAsync(settings, cancellationToken);
    }

    public async Task<GetSettingsResponse?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        var update = await service.GetUpdateAsync(checkpointDate, cancellationToken);

        return update is null ? null : mapper.Map<GetSettingsResponse>(update);
    }

    public async Task CreateAsync(CreateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(new SettingsId(request.Id), cancellationToken);

        if (settings is not null)
            return;

        await service.CreateAsync(new Settings(new SettingsId(request.Id), request.InstitutionName, request.Address,
            request.PhoneNumber, request.Tax, request.Profit), cancellationToken);
    }
}