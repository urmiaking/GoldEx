using GoldEx.Sdk.Server.Infrastructure.DTOs;

namespace GoldEx.Sdk.Server.Infrastructure.Abstractions;

public interface IPriceFetcher
{
    Task<List<PriceResponse>> GetPriceAsync(CancellationToken cancellationToken = default);
    Task<List<PriceResponse>> GetCoinPriceAsync(CancellationToken cancellationToken = default);
    Task<List<PriceResponse>> GetGoldPriceAsync(CancellationToken cancellationToken = default);
    Task<List<PriceResponse>> GetCurrencyPriceAsync(CancellationToken cancellationToken = default);
    Task<PriceResponse?> GetGram18PriceAsync(CancellationToken cancellationToken = default);
    Task<PriceResponse?> GetDollarPriceAsync(CancellationToken cancellationToken = default);
}