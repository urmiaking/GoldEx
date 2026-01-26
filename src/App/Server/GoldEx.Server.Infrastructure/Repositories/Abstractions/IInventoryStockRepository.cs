using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInventoryStockRepository : IRepository<InventoryStock>,
    ICreateRepository<InventoryStock>,
    IDeleteRepository<InventoryStock>
{
    Task<decimal> GetQuantityAsync(ProductId productId, CancellationToken cancellationToken = default);
    Task<decimal> GetQuantityAsync(CoinInstanceId coinId, CancellationToken cancellationToken = default);
    Task<decimal> GetQuantityAsync(PriceUnitId currencyId, CancellationToken cancellationToken = default);

    Task<Dictionary<ProductId, decimal>> GetQuantitiesAsync(IEnumerable<ProductId> productIds, CancellationToken cancellationToken = default);
    Task<Dictionary<CoinInstanceId, decimal>> GetQuantitiesAsync(IEnumerable<CoinInstanceId> coinIds, CancellationToken cancellationToken = default);
    Task<Dictionary<PriceUnitId, decimal>> GetQuantitiesAsync(IEnumerable<PriceUnitId> currencyIds, CancellationToken cancellationToken = default);

    Task<(List<InventorySummaryData> Data, int Total)> GetInventorySummaryAsync(RequestFilter filter, InventoryFilter inventoryFilter, 
        CancellationToken cancellationToken = default);

    Task<(List<InventorySummaryData> Data, int Total)> GetAvailableInventorySummaryAsync(RequestFilter filter,
        CalculatorFilterRequest calculatorFilter,
        CancellationToken cancellationToken = default);

    Task<List<InventoryWeightChartData>> GetInventoryWeightChartDataAsync(WarehouseActionType actionType, CancellationToken cancellationToken = default);
    Task<List<InventorySummaryData>> GetProductsReportAsync(ProductInventoryRpRequest request, CancellationToken cancellationToken = default);
    Task<List<InventorySummaryData>> GetCoinsReportAsync(CoinInventoryRpRequest request, CancellationToken cancellationToken = default);
    Task<List<InventorySummaryData>> GetCurrenciesReportAsync(CurrencyInventoryRpRequest request, CancellationToken cancellationToken = default);
}