using GoldEx.Server.Application.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.BackgroundServices;

public class PriceHistoryCleanupBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<PriceHistoryCleanupBackgroundService> logger) : BackgroundService
{
    private const string ClassName = nameof(PriceHistoryCleanupBackgroundService);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is running.", ClassName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();

                var priceHistoryService = scope.ServiceProvider.GetRequiredService<IPriceHistoryService>();

                var deletedRowsCount = await priceHistoryService.CleanupAsync(stoppingToken);

                if (deletedRowsCount > 0)
                    logger.LogInformation($"Removed {deletedRowsCount} price history");

                await Task.Delay(53_000, stoppingToken);
                //await Task.Delay(3_600_000, stoppingToken);
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