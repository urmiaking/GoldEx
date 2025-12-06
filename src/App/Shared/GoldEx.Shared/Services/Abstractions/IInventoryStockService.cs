using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInventoryStockService
{
    Task<PagedList<GetInventoryStockResponse>> GetListAsync(RequestFilter filter, InventoryFilter inventoryFilter,
        CancellationToken cancellationToken = default);

    Task<PagedList<GetInventoryStockResponse>> GetAvailableProductsAsync(CalculatorFilterRequest calculatorFilter,
        RequestFilter filter,
        CancellationToken cancellationToken = default);

    Task<List<GetInventoryWeightChartResponse>> GetInventoryWeightChartAsync(GoldUnitType targetUnit,
        CancellationToken cancellationToken = default);

    Task<PagedList<GetInventoryStockItemResponse>> GetInvoiceInventoryItemsAsync(Guid invoiceId,
        RequestFilter requestFilter,
        CancellationToken cancellationToken = default);

    Task<PagedList<GetInventoryStockTraceResponse>> GetInventoryStockTracesAsync(Guid itemId,
        ItemType itemType,
        RequestFilter requestFilter,
        CancellationToken cancellationToken = default);

    Task<GetInventoryStockAmountResponse> GetAvailableItemAmountAsync(Guid itemId,
        ItemType itemType,
        CancellationToken cancellationToken = default);

    Task DeleteProductAsync(Guid productId, CancellationToken cancellationToken = default);
}