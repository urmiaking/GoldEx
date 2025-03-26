using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Sdk.Client.Extensions;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Routings;

namespace GoldEx.Client.Services.HttpServices;

public class SettingsHttpClientService(HttpClient client, JsonSerializerOptions jsonOptions) : ISettingsHttpClientService
{
    public async Task<GetSettingsResponse?> GetAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Settings.Get(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetSettingsResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetSettingsResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Settings.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetSettingsResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.PutAsJsonAsync(ApiUrls.Settings.Update(id), request, jsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw HttpRequestFailedException.GetException(response.StatusCode, response);

            return true;
        }
        catch (Exception e)
        {
            if (e is HttpRequestException httpRequestException && httpRequestException.IsConnectionRefused())
                return false; // server is not available

            throw;
        }
    }

    public async Task<GetSettingsResponse?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.GetAsync(ApiUrls.Settings.GetUpdate(checkpointDate), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw HttpRequestFailedException.GetException(response.StatusCode, response);

            if (response.StatusCode == HttpStatusCode.NoContent)
                return null; // there is no update

            var result = await response.Content.ReadFromJsonAsync<GetSettingsResponse>(jsonOptions, cancellationToken);

            return result ?? throw new UnexpectedHttpResponseException();
        }
        catch (Exception e)
        {
            if (e is HttpRequestException httpRequestException && httpRequestException.IsConnectionRefused())
                return null; // server is not available

            throw;
        }
    }
}