using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.Settings;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class SettingService(ISettingRepository repository,
    IFileService fileService,
    IWebHostEnvironment webHostEnvironment,
    IMapper mapper,
    CreateSettingRequestValidator createValidator) : ISettingService,
    IServerSettingService
{
    #region ServerSettingService

    public async Task CreateAsync(CreateSettingRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var exists = await repository.ExistsAsync(new SettingsDefaultSpecification(), cancellationToken);

        if (exists)
            throw new InvalidOperationException("Settings already created");

        var setting = Setting.Create(request.InstitutionName,
            request.Address,
            request.PhoneNumber,
            request.TaxPercent,
            request.GoldProfitPercent,
            request.JewelryProfitPercent,
            request.GoldSafetyMarginPercent,
            request.PriceUpdateInterval);

        await repository.CreateAsync(setting, cancellationToken);
    }

    #endregion

    #region SettingService

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
        item.SetGoldSafetyMargin(request.GoldSafetyMarginPercent);

        await repository.UpdateAsync(item, cancellationToken);

        if (request.IconContent is not null)
            await fileService.ReplaceLocalFileAsync(webHostEnvironment.GetAppIconPath(), request.IconContent, cancellationToken);
    }

    #endregion
}