using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using System.Text.Json;
using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Client.Offline.Domain.PriceHistoryAggregate;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.DTOs.Prices;
using MapsterMapper;

namespace GoldEx.Client.Components.Services;

public class PriceClientService(
    HttpClient client,
    JsonSerializerOptions jsonOptions,
    IMapper mapper,
    IPriceService<Price, PriceHistory> priceService) : IPriceClientService
{
    public async Task<List<GetPriceResponse>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        var offlineList = await priceService.GetLatestPricesAsync(cancellationToken);

        try
        {
            using var response = await client.GetAsync(ApiUrls.Price.GetLatestPrices(), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw HttpRequestFailedException.GetException(response.StatusCode, response);

            var result = await response.Content.ReadFromJsonAsync<List<GetPriceResponse>>(jsonOptions, cancellationToken);

            return result ?? throw new UnexpectedHttpResponseException();
        }
        catch (HttpRequestException ex)
        {
            if (ex.InnerException != null && ex.InnerException.Message.Contains("Failed to fetch"))
            {
                // throw new HttpRequestFailedToFetchException();
                Console.WriteLine("You're offline!");
            }
        }

        return mapper.Map<List<GetPriceResponse>>(offlineList);
    }
}