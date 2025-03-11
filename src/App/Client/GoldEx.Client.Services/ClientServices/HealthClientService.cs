using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Health;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Services.ClientServices;

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