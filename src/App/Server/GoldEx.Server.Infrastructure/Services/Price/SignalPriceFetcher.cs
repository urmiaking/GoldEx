using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.DTOs;
using GoldEx.Server.Infrastructure.Services.Price.DTOs.Signal;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Http;

namespace GoldEx.Server.Infrastructure.Services.Price;

public class SignalPriceFetcher(HttpClient httpClient, JsonSerializerOptions options) : IPriceFetcher
{
    public async Task<List<PriceResponse>> GetPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = SignalPayloadItem.CreateDefaultPayload();

            var response = await httpClient.PostAsJsonAsync(ExternalRoutes.Signal, payload, options, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new BadHttpRequestException("bad request");
                    
            var content = await response.Content.ReadFromJsonAsync<SignalApiResponse>(cancellationToken);

            return SignalApiResponseMapper.MapPrices(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<PriceResponse>> GetCoinPriceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = SignalPayloadItem.CreateCoinPayload();

            var response = await httpClient.PostAsJsonAsync(ExternalRoutes.Signal, payload, options, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new BadHttpRequestException("bad request");

            var content = await response.Content.ReadFromJsonAsync<SignalApiResponse>(cancellationToken);

            return SignalApiResponseMapper.MapPrices(content);
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
            var payload = SignalPayloadItem.CreateGoldPayload();

            var response = await httpClient.PostAsJsonAsync(ExternalRoutes.Signal, payload, options, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new BadHttpRequestException("bad request");

            var content = await response.Content.ReadFromJsonAsync<SignalApiResponse>(cancellationToken);

            return SignalApiResponseMapper.MapPrices(content);
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
            var payload = SignalPayloadItem.CreateCurrencyPayload();

            var response = await httpClient.PostAsJsonAsync(ExternalRoutes.Signal, payload, options, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new BadHttpRequestException("bad request");

            var content = await response.Content.ReadFromJsonAsync<SignalApiResponse>(cancellationToken);

            return SignalApiResponseMapper.MapPrices(content);
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
            var payload = SignalPayloadItem.CreateGoldPayload();

            var response = await httpClient.PostAsJsonAsync(ExternalRoutes.Signal, payload, options, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new BadHttpRequestException("bad request");

            var content = await response.Content.ReadFromJsonAsync<SignalApiResponse>(cancellationToken);

            return SignalApiResponseMapper.GetGram18Price(content);
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
            var payload = SignalPayloadItem.CreateCurrencyPayload();

            var response = await httpClient.PostAsJsonAsync(ExternalRoutes.Signal, payload, options, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw new BadHttpRequestException("bad request");

            var content = await response.Content.ReadFromJsonAsync<SignalApiResponse>(cancellationToken);

            return SignalApiResponseMapper.GetDollarPrice(content);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}