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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                var startBlock = new TimeSpan(23, 0, 0);
                var endBlock = new TimeSpan(7, 30, 0);

                if (now >= startBlock || now < endBlock)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    continue;
                }

                using var scope = serviceScopeFactory.CreateScope();

                var orchestrator = scope.ServiceProvider.GetRequiredService<IPriceUpdateOrchestrator>();
                var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();

                var setting = await settingService.GetAsync(stoppingToken);
                var updateInterval = setting?.PriceUpdateInterval ?? TimeSpan.FromMinutes(1);

                await orchestrator.UpdateAllAsync(stoppingToken);
                await Task.Delay(updateInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "{Name} iteration failed due to: ({Message}). Will retry.", ClassName, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Name} is stopping.", ClassName);
        return base.StopAsync(cancellationToken);
    }
}