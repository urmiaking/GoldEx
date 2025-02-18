using System.Net.Http.Json;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using System.Text.Json;

namespace GoldEx.Client.Components.Services;

public class PriceClientService(HttpClient client, JsonSerializerOptions jsonOptions) : IPriceClientService
{
    public async Task<List<GetPriceResponse>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Price.GetLatestPrices(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetPriceResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}