using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;
using GoldEx.Shared.DTOs.FinancialAccounts;

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

    public async Task<PagedList<GetInventoryStockResponse>> GetAvailableProductsAsync(CalculatorFilterRequest calculatorFilter, RequestFilter filter,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.InventoryStocks.GetAvailableProducts(calculatorFilter, filter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetInventoryStockResponse>>(jsonOptions, cancellationToken);

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

    public async Task<PagedList<GetInventoryStockItemResponse>> GetInvoiceInventoryItemsAsync(Guid invoiceId,
        RequestFilter requestFilter, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.InventoryStocks.GetInvoiceInventoryItems(invoiceId, requestFilter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetInventoryStockItemResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<PagedList<GetInventoryStockTraceResponse>> GetInventoryStockTracesAsync(Guid itemId, ItemType itemType, RequestFilter requestFilter,
        CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.InventoryStocks.GetTraces(itemId, itemType, requestFilter), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<PagedList<GetInventoryStockTraceResponse>>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<GetInventoryStockAmountResponse> GetAvailableItemAmountAsync(Guid itemId, ItemType itemType, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.InventoryStocks.GetAvailableItemAmount(itemId, itemType), cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw HttpRequestFailedException.GetException(response.StatusCode, response);

        var result = await response.Content.ReadFromJsonAsync<GetInventoryStockAmountResponse>(jsonOptions, cancellationToken);

        return result ?? throw new UnexpectedHttpResponseException();
    }
}