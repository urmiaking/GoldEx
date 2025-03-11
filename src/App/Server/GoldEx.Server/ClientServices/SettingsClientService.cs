using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.SettingsAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;
using MapsterMapper;

namespace GoldEx.Server.ClientServices;

[ScopedService]
public class SettingsClientService(ISettingsService<Settings> service, IMapper mapper) : ISettingsClientService
{
    public async Task<GetSettingsResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetSettingsResponse>(settings);
    }

    public async Task<GetSettingsResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(new SettingsId(id), cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetSettingsResponse>(settings);
    }

    public async Task UpdateAsync(Guid id, UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetAsync(new SettingsId(id), cancellationToken) ?? throw new NotFoundException();

        settings.SetInstitutionName(request.InstitutionName);
        settings.SetAddress(request.Address);
        settings.SetPhoneNumber(request.PhoneNumber);
        settings.SetTax(request.Tax);
        settings.SetProfit(request.Profit);

        await service.UpdateAsync(settings, cancellationToken);
    }

    public async Task<GetSettingsResponse?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        var settings = await service.GetUpdateAsync(checkpointDate, cancellationToken);

        return settings is null ? null : mapper.Map<GetSettingsResponse>(settings);
    }
}