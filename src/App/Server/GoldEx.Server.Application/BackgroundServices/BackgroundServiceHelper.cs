using GoldEx.Sdk.Server.Application.Extensions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Shared.Application.Services.Abstractions;

namespace GoldEx.Server.Application.BackgroundServices;

public static class BackgroundServiceHelper
{
    public static async Task AddOrUpdatePrices(List<PriceResponse> apiPriceItems, List<Price> databasePriceItems,
        IPriceService<Price, PriceHistory> priceService, CancellationToken stoppingToken = default)
    {
        foreach (var apiPrice in apiPriceItems)
        {
            var dbPrice = databasePriceItems.FirstOrDefault(x => x.Title == apiPrice.Title);

            if (dbPrice is null)
            {
                string? imageFileBase64Content = null;

                if (!string.IsNullOrEmpty(apiPrice.IconUrl))
                {
                    var (base64String, contentType) = await ImageToBase64Converter.ConvertImageUrlToBase64(apiPrice.IconUrl);

                    if (base64String != null && contentType != null)
                    {
                        imageFileBase64Content = ImageToBase64Converter.GenerateBase64ImageSrc(base64String, contentType);
                    }
                }

                dbPrice = new Price(apiPrice.Title, apiPrice.MarketType, imageFileBase64Content);

                var priceHistory = new PriceHistory(apiPrice.CurrentValue, apiPrice.LastUpdate, apiPrice.Change, apiPrice.Unit);
                dbPrice.SetPriceHistory(priceHistory);

                await priceService.CreateAsync(dbPrice, stoppingToken);
            }
            else
            {
                if (dbPrice.PriceHistory.LastUpdate != apiPrice.LastUpdate)
                {
                    dbPrice.SetPriceHistory(new PriceHistory(apiPrice.CurrentValue, apiPrice.LastUpdate, apiPrice.Change, apiPrice.Unit));
                    await priceService.UpdateAsync(dbPrice, stoppingToken);
                }
            }
        }
    }
}