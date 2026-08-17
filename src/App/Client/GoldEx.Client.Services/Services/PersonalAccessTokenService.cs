using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.PersonalAccessTokens;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class PersonalAccessTokenService(HttpClient client, JsonSerializerOptions jsonOptions) : IPersonalAccessTokenService
{
    public async Task<List<PersonalAccessTokenDto>> GetListAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.PersonalAccessTokens.GetList(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<PersonalAccessTokenDto>>(jsonOptions, cancellationToken);
        return result ?? [];
    }

    public async Task<CreatePersonalAccessTokenResponse> CreateAsync(CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.PersonalAccessTokens.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<CreatePersonalAccessTokenResponse>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task RevokeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsync(ApiUrls.PersonalAccessTokens.Revoke(id), null, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.PersonalAccessTokens.Delete(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}
