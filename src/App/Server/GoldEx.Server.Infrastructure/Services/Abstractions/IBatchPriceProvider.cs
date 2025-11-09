using GoldEx.Shared.DTOs.Prices;

namespace GoldEx.Server.Infrastructure.Services.Abstractions;


/// <summary>
/// Providers should fetch multiple symbols in one call if possible.
/// Returned PriceResponse.Title should be the provider's native title; orchestrator will normalize to local titles.
/// </summary>
public interface IBatchPriceProvider
{
    Task<List<PriceResponse>> FetchAsync(IReadOnlyCollection<string> symbols, CancellationToken cancellationToken = default);
}