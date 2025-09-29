using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InventoryStockRepository(GoldExDbContext dbContext) : RepositoryBase<InventoryStock>(dbContext), IInventoryStockRepository
{
    public Task<decimal> GetQuantityAsync(ProductId productId, CancellationToken cancellationToken = default)
    {
        return Query
            .Where(stock => stock.ProductId == productId)
            .SumAsync(stock => stock.ActionType == WarehouseActionType.In
                    ? stock.ChangeAmount
                    : -stock.ChangeAmount,
                cancellationToken);
    }

    public Task<decimal> GetQuantityAsync(CoinId coinId, CancellationToken cancellationToken = default)
    {
        return Query
            .Where(stock => stock.CoinId == coinId)
            .SumAsync(stock => stock.ActionType == WarehouseActionType.In
                    ? stock.ChangeAmount
                    : -stock.ChangeAmount,
                cancellationToken);
    }

    public Task<decimal> GetQuantityAsync(PriceUnitId currencyId, CancellationToken cancellationToken = default)
    {
        return Query
            .Where(stock => stock.CurrencyId == currencyId)
            .SumAsync(stock => stock.ActionType == WarehouseActionType.In
                    ? stock.ChangeAmount
                    : -stock.ChangeAmount,
                cancellationToken);
    }

    public async Task<Dictionary<ProductId, decimal>> GetQuantitiesAsync(IEnumerable<ProductId> productIds, CancellationToken cancellationToken = default)
    {
        var ids = productIds.ToList();
        if (!ids.Any())
            return new Dictionary<ProductId, decimal>();

        return await Query
            .Where(stock => stock.ProductId != null && ids.Contains(stock.ProductId.Value))
            .GroupBy(stock => stock.ProductId!.Value)
            .Select(group => new
            {
                Id = group.Key,
                TotalQuantity = group.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
            })
            .ToDictionaryAsync(result => result.Id, result => result.TotalQuantity, cancellationToken);
    }

    public async Task<Dictionary<CoinId, decimal>> GetQuantitiesAsync(IEnumerable<CoinId> coinIds, CancellationToken cancellationToken = default)
    {
        var ids = coinIds.ToList();
        if (!ids.Any())
            return new Dictionary<CoinId, decimal>();

        return await Query
            .Where(stock => stock.CoinId != null && ids.Contains(stock.CoinId.Value))
            .GroupBy(stock => stock.CoinId!.Value)
            .Select(group => new
            {
                Id = group.Key,
                TotalQuantity = group.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
            })
            .ToDictionaryAsync(result => result.Id, result => result.TotalQuantity, cancellationToken);
    }

    public async Task<Dictionary<PriceUnitId, decimal>> GetQuantitiesAsync(IEnumerable<PriceUnitId> currencyIds, CancellationToken cancellationToken = default)
    {
        var ids = currencyIds.ToList();
        if (!ids.Any())
        {
            return new Dictionary<PriceUnitId, decimal>();
        }

        return await Query
            .Where(stock => stock.CurrencyId != null && ids.Contains(stock.CurrencyId.Value))
            .GroupBy(stock => stock.CurrencyId!.Value)
            .Select(group => new
            {
                Id = group.Key,
                TotalQuantity = group.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
            })
            .ToDictionaryAsync(result => result.Id, result => result.TotalQuantity, cancellationToken);
    }

    public async Task<(List<InventorySummaryData> Data, int Total)> GetInventorySummaryAsync(RequestFilter filter, InventoryFilter inventoryFilter, CancellationToken cancellationToken = default)
    {
        var baseQuery = Query
            .Where(x => !inventoryFilter.Start.HasValue || x.CreatedAt >= inventoryFilter.Start.Value)
            .Where(x => !inventoryFilter.End.HasValue || x.CreatedAt <= inventoryFilter.End.Value);

        switch (inventoryFilter.ItemType)
        {
            case ItemType.Product:
                {
                    var aggregationQuery = baseQuery
                        .Where(x => x.ProductId != null)
                        .GroupBy(x => x.ProductId!.Value)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            CurrentQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount),
                            SoldQuantity = g.Sum(s => s.ActionType == WarehouseActionType.Out ? s.ChangeAmount : 0),
                            DateTime = g.First().CreatedAt
                        });

                    var filteredAggregationQuery = inventoryFilter.ActionType == WarehouseActionType.In
                        ? aggregationQuery.Where(x => x.CurrentQuantity > 0)
                        : aggregationQuery.Where(x => x.SoldQuantity > 0);

                    var aggregatedResults = await filteredAggregationQuery.ToListAsync(cancellationToken);

                    if (!aggregatedResults.Any())
                    {
                        return ([], 0);
                    }

                    var productIds = aggregatedResults.Select(x => x.ProductId).ToList();
                    var products = await dbContext.Set<Product>()
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(p => p.ProductCategory)
                        .Include(x => x.WagePriceUnit)
                        .Include(x => x.StonePriceUnit)
                        .Where(p => productIds.Contains(p.Id))
                        .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

                    var saleDetails = await dbContext.Set<Invoice>()
                        .AsNoTracking()
                        .Include(x => x.ProductItems)
                            .ThenInclude(x => x.SaleWagePriceUnit)
                        .AsSplitQuery()
                        .Where(i => i.InvoiceType == InvoiceType.Sell)
                        .SelectMany(i => i.ProductItems) 
                        .Where(item => productIds.Contains(item.ProductId))
                        .ToDictionaryAsync(item => item.ProductId, item => item, cancellationToken);

                    var combinedData = aggregatedResults.Select(agg => new
                    {
                        Product = products.GetValueOrDefault(agg.ProductId),
                        SaleInfo = saleDetails.GetValueOrDefault(agg.ProductId), 
                        agg.DateTime,
                        agg.CurrentQuantity,
                        agg.SoldQuantity
                    }).Where(x => x.Product != null).AsQueryable();

                    if (!string.IsNullOrEmpty(filter.Search))
                        combinedData = combinedData.Where(x => x.Product!.Name.Contains(filter.Search) || x.Product!.Barcode.Contains(filter.Search));

                    var total = combinedData.Count();
                    var sortedData = combinedData.ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending, "Item.CreatedAt");
                    var pagedData = sortedData.Skip(filter.Skip ?? 0).Take(filter.Take ?? 100).ToList();

                    var finalData = pagedData.Select(x => new InventorySummaryData
                    {
                        Product = x.Product,
                        CurrentQuantity = x.CurrentQuantity,
                        SoldQuantity = x.SoldQuantity,
                        DateTime = x.DateTime,
                        SaleWage = x.SaleInfo?.SaleWage,
                        SaleWageType = x.SaleInfo?.SaleWageType,
                        SaleWagePriceUnitTitle = x.SaleInfo?.SaleWagePriceUnit?.Title
                    }).ToList();

                    return (finalData, total);
                }
            case ItemType.Coin:
                {
                    var aggQuery = baseQuery
                        .AsNoTracking()
                        .Where(x => x.CoinId != null)
                        .GroupBy(x => x.Coin)
                        .Select(g => new
                        {
                            Item = g.Key,
                            CurrentQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount),
                            SoldQuantity = g.Sum(s => s.ActionType == WarehouseActionType.Out ? s.ChangeAmount : 0),
                            DateTime = g.First().CreatedAt
                        });

                    var filteredAggQuery = inventoryFilter.ActionType == WarehouseActionType.In
                        ? aggQuery.Where(x => x.CurrentQuantity > 0)
                        : aggQuery.Where(x => x.SoldQuantity > 0);

                    if (!string.IsNullOrEmpty(filter.Search))
                        filteredAggQuery = filteredAggQuery.Where(x => x.Item!.Title.Contains(filter.Search));

                    var total = await filteredAggQuery.CountAsync(cancellationToken);
                    var sorted = filteredAggQuery.ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending, "Item.Title");
                    var data = await sorted.Skip(filter.Skip ?? 0).Take(filter.Take ?? 100)
                        .Select(x => new InventorySummaryData
                        {
                            Coin = x.Item,
                            CurrentQuantity = x.CurrentQuantity,
                            SoldQuantity = x.SoldQuantity,
                            DateTime = x.DateTime
                        })
                        .ToListAsync(cancellationToken);
                    return (data, total);
                }

            case ItemType.Currency:
                {
                    var aggQuery = baseQuery
                        .AsNoTracking()
                        .Where(x => x.CurrencyId != null)
                        .GroupBy(x => x.Currency!)
                        .Select(g => new
                        {
                            Item = g.Key,
                            CurrentQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount),
                            SoldQuantity = g.Sum(s => s.ActionType == WarehouseActionType.Out ? s.ChangeAmount : 0),
                            DateTime = g.Key.CreatedAt
                        });

                    var filteredAggQuery = inventoryFilter.ActionType == WarehouseActionType.In
                        ? aggQuery.Where(x => x.CurrentQuantity > 0)
                        : aggQuery.Where(x => x.SoldQuantity > 0);

                    if (!string.IsNullOrEmpty(filter.Search))
                        filteredAggQuery = filteredAggQuery.Where(x => x.Item!.Title.Contains(filter.Search));

                    var total = await filteredAggQuery.CountAsync(cancellationToken);
                    var sorted = filteredAggQuery.ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending, "Item.Title");
                    var data = await sorted.Skip(filter.Skip ?? 0).Take(filter.Take ?? 100)
                        .Select(x => new InventorySummaryData
                        {
                            Currency = x.Item,
                            CurrentQuantity = x.CurrentQuantity,
                            SoldQuantity = x.SoldQuantity,
                            DateTime = x.DateTime
                        })
                        .ToListAsync(cancellationToken);
                    return (data, total);
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(inventoryFilter.ItemType), "Invalid item type for inventory summary.");
        }
    }

    public async Task<List<Product>> GetAvailableProductsForCalculatorAsync(
        CalculatorFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var stockLevels = await Query
            .AsNoTracking()
            .Where(s => s.ProductId.HasValue)
            .GroupBy(s => s.ProductId!.Value)
            .Select(g => new
            {
                ProductId = g.Key,
                CurrentQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
            })
            .Where(s => s.CurrentQuantity > 0)
            .ToDictionaryAsync(k => k.ProductId, v => v.CurrentQuantity, cancellationToken);

        if (!stockLevels.Any())
        {
            return [];
        }

        var availableProductIds = stockLevels.Keys.ToList();

        return await dbContext.Set<Product>()
            .AsNoTracking()
            .Where(p => availableProductIds.Contains(p.Id))
            .Where(p => string.IsNullOrEmpty(filter.Name) || p.Name.Contains(filter.Name))
            .Where(p => p.ProductType == filter.ProductType)
            .Where(p => !filter.ProductCategoryId.HasValue || (p.ProductCategoryId.HasValue && p.ProductCategoryId.Value == new ProductCategoryId(filter.ProductCategoryId.Value)))
            .Where(p => !filter.Fineness.HasValue || p.Fineness == filter.Fineness.Value)
            .Where(p => !filter.MinWeight.HasValue || p.Weight >= filter.MinWeight.Value)
            .Where(p => !filter.MaxWeight.HasValue || p.Weight <= filter.MaxWeight.Value)
            .Where(p => !filter.MaxWage.HasValue || (p.WageType == WageType.Percent && p.Wage <= filter.MaxWage.Value))
            .Include(p => p.ProductCategory)
            .Include(p => p.WagePriceUnit)
            .ToListAsync(cancellationToken);
    }
}