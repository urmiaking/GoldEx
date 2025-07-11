using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.BackgroundServices;

public class PriceUpdaterBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<PriceUpdaterBackgroundService> logger) : BackgroundService
{
    private const string ClassName = nameof(PriceUpdaterBackgroundService);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is running.", ClassName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();

                var priceService = scope.ServiceProvider.GetRequiredService<IServerPriceService>();
                var priceFetcher = scope.ServiceProvider.GetRequiredService<IPriceFetcher>();
                var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();

                var setting = await settingService.GetAsync(stoppingToken);
                var updateInterval = TimeSpan.FromMinutes(1);

                if (setting is not null) 
                    updateInterval = setting.PriceUpdateInterval;

                var apiPrices = await priceFetcher.GetPriceAsync(stoppingToken);
                await priceService.AddOrUpdateAsync(apiPrices, stoppingToken);

                await Task.Delay(updateInterval, stoppingToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"{ClassName} got an exception");
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Name} is stopping.", ClassName);
        return base.StopAsync(cancellationToken);
    }
}