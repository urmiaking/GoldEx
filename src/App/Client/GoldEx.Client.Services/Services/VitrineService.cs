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
    private readonly Dictionary<string, VitrineStoreInfoDto> _storeInfoCache = new(StringComparer.OrdinalIgnoreCase);

    private string ResolveSlug(string? storeSlug)
    {
        if (!string.IsNullOrWhiteSpace(storeSlug))
            return storeSlug.Trim();

        try
        {
            var host = client.BaseAddress?.Host;
            if (!string.IsNullOrWhiteSpace(host))
                return host.Trim();
        }
        catch
        {
            // ignore
        }

        return "default";
    }

    public async Task<VitrineStoreInfoDto?> GetStoreInfoAsync(string storeSlug, CancellationToken cancellationToken = default)
    {
        var slug = ResolveSlug(storeSlug);

        if (_storeInfoCache.TryGetValue(slug, out var cachedInfo))
            return cachedInfo;

        using var response = await client.GetAsync(ApiUrls.Vitrine.GetStoreInfo(slug), cancellationToken);

        if (response.StatusCode is HttpStatusCode.NotFound)
            return null;

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var info = await response.Content.ReadFromJsonAsync<VitrineStoreInfoDto>(jsonOptions, cancellationToken);
        if (info != null)
        {
            _storeInfoCache[slug] = info;
            if (!string.IsNullOrWhiteSpace(info.Slug))
                _storeInfoCache[info.Slug] = info;
            if (!string.IsNullOrWhiteSpace(info.CustomDomain))
                _storeInfoCache[info.CustomDomain] = info;
        }

        return info;
    }

    public async Task<IReadOnlyList<VitrineProductSummaryDto>> GetVitrineProductsAsync(
        string storeSlug,
        Guid? categoryId = null,
        bool? onlyFeatured = null,
        CancellationToken cancellationToken = default)
    {
        var slug = ResolveSlug(storeSlug);
        using var response = await client.GetAsync(ApiUrls.Vitrine.GetProducts(slug, categoryId, onlyFeatured), cancellationToken);

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
        var slug = ResolveSlug(storeSlug);
        using var response = await client.GetAsync(ApiUrls.Vitrine.GetProductDetail(slug, barcode), cancellationToken);

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
        var slug = ResolveSlug(storeSlug);
        using var response = await client.GetAsync(ApiUrls.Vitrine.GetCategories(slug), cancellationToken);

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
