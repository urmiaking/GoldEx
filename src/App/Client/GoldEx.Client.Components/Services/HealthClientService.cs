using System.Net.Http.Json;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using System.Text.Json;

namespace GoldEx.Client.Components.Services;

public class HealthClientService(HttpClient client, JsonSerializerOptions jsonOptions) : IHealthClientService
{
    public async Task<HealthCheckResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Health.Get(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<HealthCheckResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}