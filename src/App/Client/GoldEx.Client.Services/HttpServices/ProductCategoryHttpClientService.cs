﻿using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Client.Abstractions.HttpServices;
using GoldEx.Sdk.Client.Extensions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Categories;
using GoldEx.Shared.Routings;

namespace GoldEx.Client.Services.HttpServices;

[ScopedService]
public class ProductCategoryHttpClientService(HttpClient client, JsonSerializerOptions jsonOptions) : IProductCategoryHttpClientService
{
    public async Task<List<GetCategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.ProductCategories.GetAll(), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetCategoryResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetCategoryResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.ProductCategories.Get(id), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetCategoryResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<bool> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.PostAsJsonAsync(ApiUrls.ProductCategories.Create(), request, jsonOptions, cancellationToken);

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

    public async Task<bool> UpdateAsync(Guid id, UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.PutAsJsonAsync(ApiUrls.ProductCategories.Update(id), request, jsonOptions, cancellationToken);

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

    public async Task<bool> DeleteAsync(Guid id, bool deletePermanently = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.DeleteAsync(ApiUrls.ProductCategories.Delete(id), cancellationToken);

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

    public async Task<List<GetPendingCategoryResponse>> GetPendingsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await client.GetAsync(ApiUrls.ProductCategories.GetPendingItems(checkpointDate), cancellationToken);

            if (!response.IsSuccessStatusCode)
                throw HttpRequestFailedException.GetException(response.StatusCode, response);

            var result = await response.Content.ReadFromJsonAsync<List<GetPendingCategoryResponse>>(jsonOptions, cancellationToken);

            return result ?? throw new UnexpectedHttpResponseException();
        }
        catch (Exception e)
        {
            if (e is HttpRequestException httpRequestException && httpRequestException.IsConnectionRefused())
                return []; // server is not available
            
            throw;
        }
    }
}