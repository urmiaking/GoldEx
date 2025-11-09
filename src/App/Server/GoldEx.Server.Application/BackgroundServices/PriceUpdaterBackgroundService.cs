using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.BackgroundServices;

public class PriceUpdaterBackgroundService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<PriceUpdaterBackgroundService> logger) : BackgroundService
{
    private const string ClassName = nameof(PriceUpdaterBackgroundService);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is running.", ClassName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now.TimeOfDay;
                var startBlock = new TimeSpan(22, 0, 0);
                var endBlock = new TimeSpan(8, 0, 0);

                // Check if current time is between 22:00 and 08:00
                if (now >= startBlock || now < endBlock)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }

                using var scope = serviceScopeFactory.CreateScope();

                var orchestrator = scope.ServiceProvider.GetRequiredService<IPriceUpdateOrchestrator>();
                var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();

                var setting = await settingService.GetAsync(stoppingToken);
                var updateInterval = TimeSpan.FromMinutes(1);

                if (setting is not null)
                    updateInterval = setting.PriceUpdateInterval;

                await orchestrator.UpdateAllAsync(stoppingToken);

                await Task.Delay(updateInterval, stoppingToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "{Name} got an exception", ClassName);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Name} is stopping.", ClassName);
        return base.StopAsync(cancellationToken);
    }
}