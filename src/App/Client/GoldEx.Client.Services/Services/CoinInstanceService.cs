using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class CoinInstanceService(HttpClient client, JsonSerializerOptions jsonOptions) : ICoinInstanceService
{
    public async Task<GetCoinInstanceResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.CoinInstances.Get(barcode), cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetCoinInstanceResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}