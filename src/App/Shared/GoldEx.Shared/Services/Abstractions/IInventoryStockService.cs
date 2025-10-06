using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

public interface IInventoryStockService
{
    Task<PagedList<GetInventoryStockResponse>> GetListAsync(RequestFilter filter, InventoryFilter inventoryFilter,
        CancellationToken cancellationToken = default);
    Task<List<GetInventoryStockResponse>> GetAvailableProductsAsync(CalculatorFilterRequest filter, CancellationToken cancellationToken = default);
    Task<List<GetInventoryWeightChartResponse>> GetInventoryWeightChartAsync(GoldUnitType targetUnit, CancellationToken cancellationToken = default);
}