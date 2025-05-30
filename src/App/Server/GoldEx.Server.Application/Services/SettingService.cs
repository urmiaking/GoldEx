using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class SettingService(ISettingsRepository repository, IMapper mapper) : ISettingService
{
    public async Task<GetSettingResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new SettingsDefaultSpecification()).FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : mapper.Map<GetSettingResponse>(item);
    }

    public async Task UpdateAsync(UpdateSettingRequest request, CancellationToken cancellationToken = default)
    {
        var item = await repository.Get(new SettingsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetAddress(request.Address);
        item.SetInstitutionName(request.InstitutionName);
        item.SetPhoneNumber(request.PhoneNumber);
        item.SetTax(request.TaxPercent);
        item.SetGoldProfit(request.GoldProfitPercent);
        item.SetJewelryProfit(request.JewelryProfitPercent);
        item.SetPriceUpdateInterval(request.PriceUpdateInterval);

        await repository.UpdateAsync(item, cancellationToken);
    }
}