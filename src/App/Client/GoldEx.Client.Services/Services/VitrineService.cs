using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.Vitrine;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class VitrineService(HttpClient client, JsonSerializerOptions jsonOptions) : IVitrineService
{
    public async Task<VitrineStoreInfoDto?> GetStoreInfoAsync(string storeSlug, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Vitrine.GetStoreInfo(storeSlug), cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        return await response.Content.ReadFromJsonAsync<VitrineStoreInfoDto>(jsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<VitrineProductSummaryDto>> GetVitrineProductsAsync(
        string storeSlug,
        Guid? categoryId = null,
        bool? onlyFeatured = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Vitrine.GetProducts(storeSlug, categoryId, onlyFeatured), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<VitrineProductSummaryDto>>(jsonOptions, cancellationToken);
        return result ?? [];
    }

    public async Task<VitrineProductDetailDto?> GetProductDetailAsync(
        string storeSlug,
        string barcode,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Vitrine.GetProductDetail(storeSlug, barcode), cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        return await response.Content.ReadFromJsonAsync<VitrineProductDetailDto>(jsonOptions, cancellationToken);
    }

    public async Task<IReadOnlyList<VitrineCategoryDto>> GetCategoriesAsync(
        string storeSlug,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.Vitrine.GetCategories(storeSlug), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<VitrineCategoryDto>>(jsonOptions, cancellationToken);
        return result ?? [];
    }

    public async Task UpdateProductVitrineAsync(
        Guid productId,
        UpdateProductVitrineRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.PutAsJsonAsync(ApiUrls.Vitrine.UpdateProductVitrine(productId), request, jsonOptions, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);
    }
}
