using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Extensions;
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
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InventoryStockRepository(
    GoldExDbContext dbContext,
    ISettingRepository settingRepository) : RepositoryBase<InventoryStock>(dbContext),
    IInventoryStockRepository
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

    public async Task<(List<InventorySummaryData> Data, int Total)> GetInventorySummaryAsync(RequestFilter filter,
        InventoryFilter inventoryFilter, CancellationToken cancellationToken = default)
    {
        var settings = await settingRepository
            .Get(new SettingsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken);

        var gramPerMesghal = settings?.GramPerMesghal ?? 4.6083m;

        var baseQuery = Query
            .Where(x => !inventoryFilter.Start.HasValue || x.CreatedAt >= inventoryFilter.Start.Value)
            .Where(x => !inventoryFilter.End.HasValue || x.CreatedAt <= inventoryFilter.End.Value);

        switch (inventoryFilter.ItemType)
        {
            case ItemType.Product:
            case ItemType.MoltenGold:
            case ItemType.UsedProduct:
                {
                    var productTypes = inventoryFilter.ItemType switch
                    {
                        ItemType.Product => new[] { ProductType.Jewelry, ProductType.Gold },
                        ItemType.MoltenGold => new[] { ProductType.MoltenGold },
                        ItemType.UsedProduct => new[] { ProductType.UsedGold },
                        _ => throw new ArgumentOutOfRangeException(nameof(inventoryFilter.ItemType))
                    };

                    if (inventoryFilter.CategoryId.HasValue)
                    {
                        baseQuery = baseQuery.Where(x => x.Product!.ProductCategoryId == new ProductCategoryId(inventoryFilter.CategoryId.Value));
                    }

                    var aggregationQuery = baseQuery
                        .Include(x => x.Product)
                        .Where(x => x.ProductId != null && productTypes.Contains(x.Product!.ProductType))
                        .GroupBy(x => x.ProductId!.Value)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            CurrentQuantity = g.Sum(s =>
                                ((s.MoltenGoldDetail != null && s.Product!.ProductType == ProductType.MoltenGold)
                                    ? s.MoltenGoldDetail.WeightUnitType
                                    : s.Product!.GoldUnitType) == GoldUnitType.Mesghal
                                    ? (s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount) * gramPerMesghal
                                    : (s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
                            ),
                            SoldQuantity = g.Where(s => s.ActionType == WarehouseActionType.Out)
                                .Sum(s =>
                                    ((s.MoltenGoldDetail != null && s.Product!.ProductType == ProductType.MoltenGold)
                                        ? s.MoltenGoldDetail.WeightUnitType
                                        : s.Product!.GoldUnitType) == GoldUnitType.Mesghal
                                        ? s.ChangeAmount * gramPerMesghal
                                        : s.ChangeAmount
                                ),
                            DateTime = g.Min(s => s.CreatedAt)
                        });

                    var filteredAggregationQuery = inventoryFilter.ActionType == WarehouseActionType.In
                        ? aggregationQuery.Where(x => x.CurrentQuantity > 0)
                        : aggregationQuery.Where(x => x.SoldQuantity > 0);

                    var aggregatedResults = await filteredAggregationQuery.ToListAsync(cancellationToken);

                    if (!aggregatedResults.Any())
                        return ([], 0);

                    var productIds = aggregatedResults.Select(x => x.ProductId).ToList();
                    var products = await dbContext.Set<Product>()
                        .AsNoTracking()
                        .Include(p => p.ProductCategory)
                        .Include(x => x.MoltenGold!.Assayer)
                        .Where(p => productIds.Contains(p.Id))
                        .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

                    var saleDetails = await dbContext.Set<Invoice>()
                        .AsNoTracking()
                        .Include(i => i.ProductItems)
                        .ThenInclude(pi => pi.SaleWagePriceUnit)
                        .Where(i => i.InvoiceType == InvoiceType.Sell)
                        .SelectMany(i => i.ProductItems
                            .Select(pi => new
                            {
                                pi.ProductId,
                                pi.SaleWage,
                                pi.SaleWageType,
                                pi.SaleWagePriceUnit,
                                pi.CreatedAt
                            }))
                        .Where(item => productIds.Contains(item.ProductId))
                        .GroupBy(item => item.ProductId)
                        .Select(g => g
                            .OrderByDescending(x => x.CreatedAt)
                            .First())
                        .ToDictionaryAsync(x => x.ProductId, x => x, cancellationToken);

                    var combinedData = aggregatedResults.Select(agg => new
                    {
                        Product = products.GetValueOrDefault(agg.ProductId),
                        SaleInfo = saleDetails.GetValueOrDefault(agg.ProductId),
                        agg.DateTime,
                        agg.CurrentQuantity,
                        agg.SoldQuantity
                    }).Where(x => x.Product != null).AsQueryable();

                    if (!string.IsNullOrEmpty(filter.Search))
                        combinedData = combinedData.Where(x =>
                            x.Product!.Name.Contains(filter.Search) ||
                            x.Product!.Barcode.Contains(filter.Search));

                    var total = combinedData.Count();
                    var sortedData = combinedData.ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending, "Product.CreatedAt");
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

    public async Task<(List<InventorySummaryData> Data, int Total)> GetAvailableInventorySummaryAsync(RequestFilter filter,
    CalculatorFilterRequest calculatorFilter,
    CancellationToken cancellationToken = default)
    {
        var settings = await settingRepository
             .Get(new SettingsDefaultSpecification())
             .FirstOrDefaultAsync(cancellationToken);

        var gramPerMesghal = settings?.GramPerMesghal ?? 4.6083m;

        var baseQuery = Query
            .AsNoTracking()
            .Where(x => x.ProductId != null && x.Product!.ProductType == calculatorFilter.ProductType);

        var aggregationQuery = baseQuery
                        .Include(x => x.Product!.ProductCategory)
                        .Where(x => x.ProductId != null && x.Product!.ProductType == calculatorFilter.ProductType)
                        .GroupBy(x => x.ProductId!.Value)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            CurrentQuantity = g.Sum(s =>
                                ((s.MoltenGoldDetail != null && s.Product!.ProductType == ProductType.MoltenGold)
                                    ? s.MoltenGoldDetail.WeightUnitType
                                    : s.Product!.GoldUnitType) == GoldUnitType.Mesghal
                                    ? (s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount) * gramPerMesghal
                                    : (s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
                            ),
                            SoldQuantity = g.Where(s => s.ActionType == WarehouseActionType.Out)
                                .Sum(s =>
                                    ((s.MoltenGoldDetail != null && s.Product!.ProductType == ProductType.MoltenGold)
                                        ? s.MoltenGoldDetail.WeightUnitType
                                        : s.Product!.GoldUnitType) == GoldUnitType.Mesghal
                                        ? s.ChangeAmount * gramPerMesghal
                                        : s.ChangeAmount
                                ),
                            DateTime = g.Min(s => s.CreatedAt)
                        });

        var filteredAggregationQuery = aggregationQuery.Where(x => x.CurrentQuantity > 0);

        var aggregatedResults = await filteredAggregationQuery.ToListAsync(cancellationToken);

        if (!aggregatedResults.Any())
            return ([], 0);

        var productIds = aggregatedResults.Select(x => x.ProductId).ToList();

        var products = await dbContext.Set<Product>()
            .AsNoTracking()
            .Include(p => p.ProductCategory)
            .Where(p => productIds.Contains(p.Id))
            .Where(p => string.IsNullOrEmpty(calculatorFilter.Name) || p.Name.Contains(calculatorFilter.Name))
            .Where(p => !calculatorFilter.Fineness.HasValue || p.Fineness == calculatorFilter.Fineness.Value)
            .Where(p => !calculatorFilter.MaxWage.HasValue || (p.WageType == WageType.Percent && p.Wage <= calculatorFilter.MaxWage.Value))
            .Where(p => !calculatorFilter.ProductCategoryId.HasValue || (p.ProductCategoryId.HasValue && p.ProductCategoryId.Value == new ProductCategoryId(calculatorFilter.ProductCategoryId.Value)))
            .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

        var filteredAggregatedResults = aggregatedResults
            .Where(agg => !calculatorFilter.MinWeight.HasValue || agg.CurrentQuantity >= calculatorFilter.MinWeight.Value)
            .Where(agg => !calculatorFilter.MaxWeight.HasValue || agg.CurrentQuantity <= calculatorFilter.MaxWeight.Value)
            .ToList();

        if (!filteredAggregatedResults.Any())
            return ([], 0);

        var combinedData = filteredAggregatedResults.Select(agg => new
        {
            Product = products.GetValueOrDefault(agg.ProductId),
            agg.DateTime,
            agg.CurrentQuantity,
            agg.SoldQuantity
        }).Where(x => x.Product != null).AsQueryable();

        if (!string.IsNullOrEmpty(filter.Search))
            combinedData = combinedData.Where(x =>
                x.Product!.Name.Contains(filter.Search) ||
                x.Product!.Barcode.Contains(filter.Search));

        var total = combinedData.Count();
        var sortedData = combinedData.ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending,
            "Product.CreatedAt");
        var pagedData = sortedData.Skip(filter.Skip ?? 0).Take(filter.Take ?? 100).ToList();
        var finalData = pagedData.Select(x => new InventorySummaryData
        {
            Product = x.Product,
            CurrentQuantity = x.CurrentQuantity,
            SoldQuantity = x.SoldQuantity,
            DateTime = x.DateTime
        }).ToList();

        return (finalData, total);
    }

    public async Task<List<InventoryWeightChartData>> GetInventoryWeightChartDataAsync(GoldUnitType targetUnit,
        CancellationToken cancellationToken = default)
    {
        var settings = await settingRepository
            .Get(new SettingsDefaultSpecification())
            .FirstOrDefaultAsync(cancellationToken);

        var gramPerMesghal = settings?.GramPerMesghal ?? 4.6083m;

        var productStocks = await dbContext.Set<InventoryStock>()
            .AsNoTracking()
            .Include(x => x.Product!)
            .ThenInclude(p => p.ProductCategory)
            .Where(x => x.ProductId != null)
            .ToListAsync(cancellationToken);

        if (productStocks.Count == 0)
            return [];

        var groupedByProduct = productStocks
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                g.First().Product,
                CurrentQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
            })
            .Where(x => x.CurrentQuantity > 0)
            .ToList();

        var goldWeight = groupedByProduct
            .Where(x => x.Product is { ProductType: ProductType.Gold })
            .Sum(x => ConvertToTarget(x.CurrentQuantity * x.Product!.Weight, x.Product!.GoldUnitType));

        var jewelryWeight = groupedByProduct
            .Where(x => x.Product is { ProductType: ProductType.Jewelry })
            .Sum(x => ConvertToTarget(x.CurrentQuantity * x.Product!.Weight, x.Product!.GoldUnitType));

        var result = new List<InventoryWeightChartData>();

        if (goldWeight > 0)
            result.Add(new InventoryWeightChartData(ProductType.Gold.GetDisplayName(), goldWeight, targetUnit));

        if (jewelryWeight > 0)
            result.Add(new InventoryWeightChartData(ProductType.Jewelry.GetDisplayName(), jewelryWeight, targetUnit));

        return result;

        decimal ConvertToTarget(decimal value, GoldUnitType from)
        {
            if (from == targetUnit)
                return value;

            return from switch
            {
                GoldUnitType.Mesghal when targetUnit == GoldUnitType.Gram => value * gramPerMesghal,
                GoldUnitType.Gram when targetUnit == GoldUnitType.Mesghal => value / gramPerMesghal,
                _ => value
            };
        }
    }
}