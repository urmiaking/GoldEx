using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.SmsTemplates;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class SmsTemplateService(HttpClient client, JsonSerializerOptions jsonOptions) : ISmsTemplateService
{
    public async Task<List<SmsTemplateResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.SmsTemplates.GetList(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<SmsTemplateResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task UpdateAsync(List<SmsTemplateRequest> requests, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.SmsTemplates.Update(), requests, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}