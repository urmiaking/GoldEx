using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class BarcodePrintSettingsService(HttpClient client, JsonSerializerOptions jsonOptions)
    : IBarcodePrintSettingsService
{
    public async Task<GetBarcodePrintSettingsResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Settings.GetBarcodePrintSettings(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetBarcodePrintSettingsResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task UpdateAsync(UpdateBarcodePrintSettingsRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(
            ApiUrls.Settings.UpdateBarcodePrintSettings(),
            request,
            jsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}