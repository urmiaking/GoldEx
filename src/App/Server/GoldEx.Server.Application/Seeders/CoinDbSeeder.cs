using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal sealed class CoinDbSeeder(ICoinService coinService, ILogger<CoinDbSeeder> logger) : IDbSeeder
{
    public int Order => 50;

    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken)
    {
        if (!context.TryGet<List<Price>>(SeedContextKeys.NewlyCreatedPrices, out var prices))
            return;

        var coinPrices = prices
            .Where(p => p.MarketType == MarketType.Coin)
            .ToList();

        foreach (var price in coinPrices)
        {
            var (weight, fineness, startYear, endYear) = GetCoinSpecs(price.Title);

            await coinService.CreateAsync(
                new CoinRequestDto(
                    null,
                    price.Title,
                    weight,
                    fineness,
                    startYear,
                    endYear,
                    price.Id!.Value),
                cancellationToken);
        }

        if (coinPrices.Any())
            logger.LogInformation($"{nameof(CoinDbSeeder)}: Seeded {coinPrices.Count} coins.");
    }

    private static (decimal Weight, decimal Fineness, int StartMintYear, int? EndMintYear) GetCoinSpecs(string title) =>
        title.Trim() switch
        {
            "سکه بهار آزادی" => (8.13598m, 900m, 1979, 1991),
            "سکه امامی" => (8.13598m, 900m, 1991, null),
            "نیم سکه" => (4.0665m, 900m, 1979, null),
            "ربع سکه" => (2.03325m, 900m, 1979, null),
            "سکه یک گرمی" => (1.01m, 900m, 2010, 2018),
            _ => (0m, 0m, 0, null)
        };
}