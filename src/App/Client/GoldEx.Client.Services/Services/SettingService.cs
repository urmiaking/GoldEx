using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class SettingService(HttpClient client, JsonSerializerOptions jsonOptions) : ISettingService
{
    public async Task<GetSettingResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Settings.Get(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetSettingResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task UpdateAsync(UpdateSettingRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.Settings.Update(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}