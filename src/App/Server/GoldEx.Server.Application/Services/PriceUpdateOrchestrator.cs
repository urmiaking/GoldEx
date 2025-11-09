using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Services.PriceProviders;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceProviderMappings;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services;

// Orchestrates minimal provider calls and updates prices via existing PriceService.
[ScopedService]
internal class PriceUpdateOrchestrator(
    IPriceProviderMappingRepository mappingRepository,
    IPriceRepository priceRepository,
    PriceProviderRegistry providerRegistry,
    IServerPriceService priceService,
    ILogger<PriceUpdateOrchestrator> logger) : IPriceUpdateOrchestrator
{
    public async Task UpdateAllAsync(CancellationToken cancellationToken = default)
    {
        var mappings = await mappingRepository
            .Get(new PriceProviderMappingsDefaultSpecification())
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (mappings.Count == 0) return;

        // Load target prices for title normalization
        var priceIds = mappings.Select(m => m.PriceId).Distinct().ToList();
        var prices = await priceRepository
            .Get(new PricesByIdsSpecification(priceIds))
            .AsNoTracking()
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // Group mappings by provider
        var groups = mappings.GroupBy(m => m.ProviderType);

        var collected = new List<PriceResponse>();

        foreach (var group in groups)
        {
            var providerType = group.Key;
            if (providerType == PriceProviderType.Manual) continue;

            var provider = providerRegistry.Resolve(providerType);
            if (provider is null)
            {
                logger.LogDebug("Provider {ProviderType} resolved to null (maybe Manual or not registered).", providerType);
                continue;
            }

            // Gather symbols for this provider
            var symbols = group.Select(g => g.ProviderSymbol).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (symbols.Count == 0) continue;

            // Fetch ticks from provider
            var ticks = await provider.FetchAsync(symbols, cancellationToken);

            if (ticks.Count == 0) continue;

            // For each mapping, find a matching tick and normalize to local title/market
            foreach (var m in group)
            {
                if (!prices.TryGetValue(m.PriceId, out var price)) continue;

                // Attempt exact match by provider symbol first, then contains match
                var tick = ticks.FirstOrDefault(t => t.Title.Equals(m.ProviderSymbol, StringComparison.OrdinalIgnoreCase))
                           ?? ticks.FirstOrDefault(t => t.Title.Contains(m.ProviderSymbol, StringComparison.OrdinalIgnoreCase));

                if (tick is null) continue;

                // Normalize to local seeded title so AddOrUpdate updates the correct Price row
                collected.Add(tick with { Title = price.Title, MarketType = price.MarketType });
            }
        }

        if (collected.Count == 0) return;

        await priceService.AddOrUpdateAsync(collected, cancellationToken);
    }
}