using System.Net.Http.Json;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Server.Infrastructure.Services.Price.DTOs;
using GoldEx.Shared.Routings;

namespace GoldEx.Server.Infrastructure.Services.Price;

public class PriceFetcher(HttpClient httpClient) : IPriceFetcher
{
    public async Task<List<PriceResponse>> GetCoinPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(ExternalRoutes.TalaIr, cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return TalaIrApiResponseMapper.MapCoinPrices(content);
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
            var response = await httpClient.GetAsync(ExternalRoutes.TalaIr, cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return TalaIrApiResponseMapper.MapGoldPrices(content);
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
            var response = await httpClient.GetAsync(ExternalRoutes.TalaIr, cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return TalaIrApiResponseMapper.MapCurrencyPrices(content);
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
            var response = await httpClient.GetAsync(ExternalRoutes.TalaIr, cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return TalaIrApiResponseMapper.GetGram18Price(content);
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
            var response = await httpClient.GetAsync(ExternalRoutes.TalaIr, cancellationToken);

            var content = await response.Content.ReadFromJsonAsync<TalaIrApiResponse>(cancellationToken);

            return TalaIrApiResponseMapper.GetDollarPrice(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}