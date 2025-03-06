﻿using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Routings;

namespace GoldEx.Client.Services.HttpServices;

public class ProductHttpClientService(
    HttpClient client,
    JsonSerializerOptions jsonOptions) : IProductHttpClientService
{
    public async Task<PagedList<GetProductResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Products.GetList(filter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetProductResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetProductResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Products.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetProductResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetProductResponse?> GetAsync(string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Products.GetByBarcode(barcode), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetProductResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.Products.Create(), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.Products.Update(id), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task DeleteAsync(Guid id, bool deletePermanently = false, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.Products.Delete(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }

    public async Task<List<GetPendingProductResponse>> GetPendingsAsync(DateTime checkpointDate,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Products.GetPendingItems(checkpointDate), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetPendingProductResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}