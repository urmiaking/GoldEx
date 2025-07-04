﻿using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Health;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class HealthService(HttpClient client, JsonSerializerOptions jsonOptions) : IHealthService
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