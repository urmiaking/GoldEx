using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Shared.DTOs.Prices;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Infrastructure.Services;

public class GenericBatchPriceProvider<TFetcher>(
    TFetcher fetcher,
    ILogger<GenericBatchPriceProvider<TFetcher>> logger) : IBatchPriceProvider
    where TFetcher : class, IPriceFetcher
{
    public async Task<List<PriceResponse>> FetchAsync(IReadOnlyCollection<string> symbols, CancellationToken cancellationToken = default)
    {
        var list = await fetcher.GetPriceAsync(cancellationToken);

        if (list.Count == 0 || symbols.Count == 0)
            return list;

        var set = symbols.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filtered = list.Where(p =>
                set.Contains(p.Title) ||
                set.Any(s => p.Title.Contains(s, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (filtered.Count == 0)
            logger.LogDebug("No matches found for symbols: {Symbols}", string.Join(", ", symbols));

        return filtered;
    }
}