using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services.Abstractions;
using GoldEx.Shared.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal class SettingsDbSeeder(
    IServerSettingService serverSettingService,
    ISettingService settingService,
    IOptions<DefaultSetting> options,
    ILogger<SettingsDbSeeder> logger) : IDbSeeder
{
    private readonly DefaultSetting _defaultSetting = options.Value;

    public int Order => 0;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        var settings = await settingService.GetAsync(cancellationToken);

        if (settings is null)
        {
            var setting = new CreateSettingRequest(_defaultSetting.InstitutionName,
                _defaultSetting.Address,
                _defaultSetting.PhoneNumber,
                _defaultSetting.TaxPercent,
                _defaultSetting.GoldProfitPercent,
                _defaultSetting.JewelryProfitPercent,
                _defaultSetting.MoltenGoldCommissionPercent,
                _defaultSetting.GoldSafetyMarginPercent,
                _defaultSetting.UsedGoldFinenessDeductionRate,
                _defaultSetting.GramPerMesghal,
                _defaultSetting.PriceUpdateInterval);

            await serverSettingService.CreateAsync(setting, cancellationToken);

            logger.LogInformation($"{nameof(SettingsDbSeeder)}: Seeded settings.");
        }
    }
}