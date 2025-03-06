﻿using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Routings;

namespace GoldEx.Client.Services.HttpServices;

public class PriceHttpClientService(
    HttpClient client,
    JsonSerializerOptions jsonOptions) : IPriceHttpClientService
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