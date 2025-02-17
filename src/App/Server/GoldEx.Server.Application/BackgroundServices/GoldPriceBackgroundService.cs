using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Sdk.Server.Infrastructure.DTOs.Enums;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Application.BackgroundServices;

public class GoldPriceBackgroundService(IServiceScopeFactory serviceScopeFactory, ILogger<GoldPriceBackgroundService> logger) : BackgroundService
{
    private const string ClassName = nameof(GoldPriceBackgroundService);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("{Name} is running.", ClassName);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = serviceScopeFactory.CreateScope();

                var priceFetcher = scope.ServiceProvider.GetRequiredService<IPriceFetcher>();

                var priceService = scope.ServiceProvider.GetRequiredService<IPriceService>();

                var list = await priceFetcher.GetGoldPriceAsync(stoppingToken);

                await AddToDb(list, priceService, stoppingToken);    
                
                await Task.Delay(10_000, stoppingToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, $"{ClassName} got an exception");
        }
    }

    private static async Task AddToDb(List<PriceResponse> items, IPriceService priceService, CancellationToken stoppingToken)
    {
        var priceItems = await priceService.GetListAsync(stoppingToken);

        foreach (var item in items)
        {
            var price = priceItems.FirstOrDefault(x => x.Title == item.Title);
            var priceHistory = new PriceHistory(item.CurrentValue, item.LastUpdate, item.DailyChangeRate);

            if (price != null)
            {
                price.AddPriceHistory(priceHistory);
                await priceService.UpdateAsync(price, stoppingToken);
            }
            else
            {
                price = new Price(item.Title, item.PriceType);
                price.AddPriceHistory(priceHistory);
                await priceService.CreateAsync(price, stoppingToken);
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("{Name} is stopping.", ClassName);
        return base.StopAsync(cancellationToken);
    }
}