using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceHistoryAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
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

                var priceService = scope.ServiceProvider.GetRequiredService<IPriceService<Price, PriceHistory>>();
                var priceHistoryService = scope.ServiceProvider.GetRequiredService<IPriceHistoryService<PriceHistory>>();
                var priceFetcher = scope.ServiceProvider.GetRequiredService<IPriceFetcher>();

                var apiPriceItems = await priceFetcher.GetPriceAsync(stoppingToken);
                var databasePriceItems = await priceService.GetListAsync(stoppingToken);

                await BackgroundServiceHelper.InsertToDb(apiPriceItems, databasePriceItems, priceService, priceHistoryService, stoppingToken);
                await Task.Delay(30_000, stoppingToken);
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