using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class InventoryStockService(HttpClient client, JsonSerializerOptions jsonOptions) : IInventoryStockService
{
    public async Task<PagedList<GetInventoryStockResponse>> GetListAsync(RequestFilter filter, InventoryFilter inventoryFilter, 
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.InventoryStocks.GetList(filter, inventoryFilter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetInventoryStockResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetInventoryStockResponse>> GetAvailableProductsAsync(CalculatorFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.InventoryStocks.GetAvailableProducts(filter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetInventoryStockResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<List<GetInventoryWeightChartResponse>> GetInventoryWeightChartAsync(GoldUnitType targetUnit,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.InventoryStocks.GetInventoryWeightChart(targetUnit), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<List<GetInventoryWeightChartResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}