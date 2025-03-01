using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceHistoryAggregate;
using GoldEx.Shared.Application.Services.Abstractions;

namespace GoldEx.Server.Application.BackgroundServices;

public static class BackgroundServiceHelper
{
    public static async Task InsertToDb(List<PriceResponse> apiPriceItems, List<Price> databasePriceItems,
        IPriceService<Price, PriceHistory> priceService, IPriceHistoryService<PriceHistory> priceHistoryService, CancellationToken stoppingToken = default)
    {
        foreach (var apiPrice in apiPriceItems)
        {
            var dbPrice = databasePriceItems.FirstOrDefault(x => x.Title == apiPrice.Title);

            if (dbPrice is null)
            {
                if (string.IsNullOrEmpty(apiPrice.Title))
                {
                    throw new InvalidDataException();
                }
                dbPrice = new Price(apiPrice.Title, apiPrice.MarketType, apiPrice.IconUrl);
                await priceService.CreateAsync(dbPrice, stoppingToken);
            }

            var priceHistory = new PriceHistory(dbPrice.Id, apiPrice.CurrentValue, apiPrice.LastUpdate, apiPrice.Change, apiPrice.Unit);
            await priceHistoryService.CreateAsync(priceHistory, stoppingToken);
        }
    }
}