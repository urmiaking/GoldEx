using System.Net.Http.Json;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Server.Infrastructure.Services.Price.DTOs;

namespace GoldEx.Server.Infrastructure.Services.Price;

[ScopedService]
public class PriceFetcher(HttpClient httpClient) : IPriceFetcher
{
    public async Task<List<PriceResponse>> GetCoinPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("https://www.tala.ir/ajax/price", cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return PriceResponseMapper.MapCoinPrices(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<PriceResponse>> GetGoldPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("https://www.tala.ir/ajax/price", cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return PriceResponseMapper.MapGoldPrices(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<PriceResponse>> GetCurrencyPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("https://www.tala.ir/ajax/price", cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return PriceResponseMapper.MapCurrencyPrices(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<PriceResponse?> GetGram18PriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("https://www.tala.ir/ajax/price", cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return PriceResponseMapper.GetGram18Price(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<PriceResponse?> GetDollarPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync("https://www.tala.ir/ajax/price", cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return PriceResponseMapper.GetDollarPrice(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}