using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
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

        // 1. Base Query
        var query = dbContext.Set<InventoryStock>().AsNoTracking();

        // 2. Apply Stock Filters
        if (inventoryFilter.Start.HasValue)
            query = query.Where(x => x.PostingDate >= inventoryFilter.Start.Value);

        if (inventoryFilter.End.HasValue)
            query = query.Where(x => x.PostingDate <= inventoryFilter.End.Value);

        if (inventoryFilter.InventoryEntryId.HasValue)
        {
            var entryId = new InventoryEntryId(inventoryFilter.InventoryEntryId.Value);
            query = query.Where(x => x.InventoryEntryId == entryId);
        }

        switch (inventoryFilter.ItemType)
        {
            case ItemType.Product:
            case ItemType.MoltenGold:
            case ItemType.UsedProduct:
                {
                    var targetProductTypes = inventoryFilter.ItemType switch
                    {
                        ItemType.Product => new[] { ProductType.Jewelry, ProductType.Gold },
                        ItemType.MoltenGold => new[] { ProductType.MoltenGold },
                        ItemType.UsedProduct => new[] { ProductType.UsedGold },
                        _ => Array.Empty<ProductType>()
                    };

                    query = query.Where(x => targetProductTypes.Contains(x.Product!.ProductType));

                    if (inventoryFilter.CategoryId.HasValue)
                    {
                        var catId = new ProductCategoryId(inventoryFilter.CategoryId.Value);
                        query = query.Where(x => x.Product!.ProductCategoryId == catId);
                    }

                    if (!string.IsNullOrEmpty(filter.Search))
                    {
                        query = query.Where(x => x.Product!.Name.Contains(filter.Search) ||
                                                 x.Product!.Barcode.Contains(filter.Search));
                    }

                    // --- 3. Aggregation (Unchanged) ---
                    var groupedQuery = query
                        .GroupBy(x => x.ProductId)
                        .Select(g => new
                        {
                            ProductId = g.Key,
                            LastActivityDate = g.Min(x => x.PostingDate),
                            CurrentQuantity = g.Sum(s =>
                                (
                                    ((s.MoltenGoldDetail != null && s.Product!.ProductType == ProductType.MoltenGold)
                                        ? s.MoltenGoldDetail.WeightUnitType
                                        : s.Product!.GoldUnitType) == GoldUnitType.Mesghal
                                        ? gramPerMesghal
                                        : 1.0m
                                ) * (s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
                            ),
                            SoldQuantity = g
                                .Where(s => s.ActionType == WarehouseActionType.Out &&
                                            s.ReverseInventoryStockId == null &&
                                            ((s.InvoiceId != null && s.Invoice!.InvoiceType == InvoiceType.Sell) || s.MeltingBatchId != null))
                                .Sum(s =>
                                    (
                                        ((s.MoltenGoldDetail != null && s.Product!.ProductType == ProductType.MoltenGold)
                                            ? s.MoltenGoldDetail.WeightUnitType
                                            : s.Product!.GoldUnitType) == GoldUnitType.Mesghal
                                            ? gramPerMesghal
                                            : 1.0m
                                    ) * s.ChangeAmount
                                )
                        });

                    if (inventoryFilter.ActionType == WarehouseActionType.In)
                        groupedQuery = groupedQuery.Where(x => x.CurrentQuantity > 0);
                    else
                        groupedQuery = groupedQuery.Where(x => x.SoldQuantity > 0);

                    var total = await groupedQuery.CountAsync(cancellationToken);

                    // --- 4. Flattening & Projection (UPDATED) ---
                    var flattenedQuery = groupedQuery.Join(
                        dbContext.Set<Product>().Include(p => p.MoltenGold.Assayer),
                        g => g.ProductId,
                        p => p.Id,
                        (g, p) => new
                        {
                            Product = p,
                            g.CurrentQuantity,
                            g.SoldQuantity,
                            g.LastActivityDate,

                            // Flattened properties for sorting
                            ProductName = p.Name,
                            ProductBarcode = p.Barcode,
                            ProductFineness = p.Fineness,
                            ProductType = p.ProductType,
                            ProductCreatedAt = p.CreatedAt,

                            // NEW: Flatten Category Title (Triggers Left Join in SQL)
                            CategoryTitle = p.ProductCategory!.Title
                        }
                    );

                    // --- 5. Manual Sorting (UPDATED) ---
                    var isDesc = filter.SortDirection == null || filter.SortDirection == SortDirection.None ||
                                 filter.SortDirection == SortDirection.Descending;

                    flattenedQuery = filter.SortLabel switch
                    {
                        // Category Title Sort
                        "Product.ProductCategory.Title" => isDesc
                            ? flattenedQuery.OrderByDescending(x => x.CategoryTitle)
                            : flattenedQuery.OrderBy(x => x.CategoryTitle),

                        // Quantity Sorts (Handled explicitly)
                        "CurrentQuantity" or "CurrentAmount" => isDesc
                            ? flattenedQuery.OrderByDescending(x => x.CurrentQuantity)
                            : flattenedQuery.OrderBy(x => x.CurrentQuantity),

                        "SoldQuantity" or "SoldAmount" => isDesc
                            ? flattenedQuery.OrderByDescending(x => x.SoldQuantity)
                            : flattenedQuery.OrderBy(x => x.SoldQuantity),

                        // Existing Sorts
                        "Product.Name" => isDesc ? flattenedQuery.OrderByDescending(x => x.ProductName) : flattenedQuery.OrderBy(x => x.ProductName),
                        "Product.Barcode" => isDesc ? flattenedQuery.OrderByDescending(x => x.ProductBarcode) : flattenedQuery.OrderBy(x => x.ProductBarcode),
                        "Product.Fineness" => isDesc ? flattenedQuery.OrderByDescending(x => x.ProductFineness) : flattenedQuery.OrderBy(x => x.ProductFineness),
                        "Product.ProductType" => isDesc ? flattenedQuery.OrderByDescending(x => x.ProductType) : flattenedQuery.OrderBy(x => x.ProductType),

                        _ => isDesc ? flattenedQuery.OrderByDescending(x => x.ProductCreatedAt) : flattenedQuery.OrderBy(x => x.ProductCreatedAt)
                    };

                    // --- 6. Paging (Unchanged) ---
                    var pagedResult = await flattenedQuery
                        .Skip(filter.Skip ?? 0)
                        .Take(filter.Take ?? 100)
                        .ToListAsync(cancellationToken);

                    if (!pagedResult.Any())
                        return ([], 0);

                    // --- 7. Details Loading (Unchanged) ---
                    var pagedProductIds = pagedResult.Select(x => x.Product.Id).ToList();

                    var rawSaleItems = await dbContext.Set<Invoice>()
                        .AsNoTracking()
                        .Where(i => i.InvoiceType == InvoiceType.Sell)
                        .SelectMany(i => i.ProductItems.Select(pi => new
                        {
                            pi.ProductId,
                            pi.SaleWage,
                            pi.SaleWageType,
                            pi.SaleWagePriceUnitId,
                            Date = i.InvoiceDate,
                            ItemId = pi.Id
                        }))
                        .Where(x => pagedProductIds.Contains(x.ProductId))
                        .ToListAsync(cancellationToken);

                    var saleDetails = rawSaleItems
                        .GroupBy(x => x.ProductId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.OrderByDescending(x => x.Date).ThenByDescending(x => x.ItemId.Value).First()
                        );

                    var unitIds = saleDetails.Values.Where(v => v.SaleWagePriceUnitId.HasValue)
                        .Select(v => v.SaleWagePriceUnitId!.Value).Distinct().ToList();

                    Dictionary<PriceUnitId, string> priceUnitTitles = [];
                    if (unitIds.Any())
                    {
                        priceUnitTitles = await dbContext.Set<PriceUnit>()
                            .AsNoTracking()
                            .Where(u => unitIds.Contains(u.Id))
                            .ToDictionaryAsync(u => u.Id, u => u.Title, cancellationToken);
                    }

                    var finalData = pagedResult.Select(x =>
                    {
                        var saleInfo = saleDetails.GetValueOrDefault(x.Product.Id);
                        string? wageUnitTitle = null;
                        if (saleInfo?.SaleWagePriceUnitId != null)
                            priceUnitTitles.TryGetValue(saleInfo.SaleWagePriceUnitId.Value, out wageUnitTitle);

                        return new InventorySummaryData
                        {
                            Product = x.Product,
                            CurrentAmount = x.CurrentQuantity,
                            SoldAmount = x.SoldQuantity,
                            DateTime = x.LastActivityDate,
                            SaleWage = saleInfo?.SaleWage,
                            SaleWageType = saleInfo?.SaleWageType,
                            SaleWagePriceUnitTitle = wageUnitTitle
                        };
                    }).ToList();

                    return (finalData, total);
                }

            case ItemType.Coin:
                {
                    if (!string.IsNullOrEmpty(filter.Search))
                        query = query.Where(x => x.Coin!.Title.Contains(filter.Search));

                    var groupedQuery = query
                        .Where(x => x.CoinId != null)
                        .GroupBy(x => x.CoinId)
                        .Select(g => new
                        {
                            CoinId = g.Key,
                            LastActivityDate = g.Min(x => x.PostingDate),
                            CurrentQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount),
                            SoldQuantity = g.Where(s => s.ActionType == WarehouseActionType.Out &&
                                                        s.InvoiceId != null &&
                                                        s.Invoice!.InvoiceType == InvoiceType.Sell &&
                                                        s.ReverseInventoryStockId == null)
                                            .Sum(s => s.ChangeAmount)
                        });

                    if (inventoryFilter.ActionType == WarehouseActionType.In)
                        groupedQuery = groupedQuery.Where(x => x.CurrentQuantity > 0);
                    else
                        groupedQuery = groupedQuery.Where(x => x.SoldQuantity > 0);

                    var total = await groupedQuery.CountAsync(cancellationToken);

                    var flattenedQuery = groupedQuery.Join(
                        dbContext.Set<Coin>(),
                        g => g.CoinId,
                        c => c.Id,
                        (g, c) => new
                        {
                            Coin = c,
                            g.CurrentQuantity,
                            g.SoldQuantity,
                            g.LastActivityDate,
                            CoinTitle = c.Title
                        }
                    );

                    var isDesc = filter.SortDirection == null || filter.SortDirection == SortDirection.None ||
                                 filter.SortDirection == SortDirection.Descending;

                    flattenedQuery = filter.SortLabel switch
                    {
                        "Coin.Title" => isDesc ? flattenedQuery.OrderByDescending(x => x.CoinTitle) : flattenedQuery.OrderBy(x => x.CoinTitle),
                        "CurrentAmount" => isDesc ? flattenedQuery.OrderByDescending(x => x.CurrentQuantity) : flattenedQuery.OrderBy(x => x.CurrentQuantity),
                        "SoldAmount" => isDesc ? flattenedQuery.OrderByDescending(x => x.SoldQuantity) : flattenedQuery.OrderBy(x => x.SoldQuantity),
                        _ => isDesc ? flattenedQuery.OrderByDescending(x => x.CoinTitle) : flattenedQuery.OrderBy(x => x.CoinTitle)
                    };

                    var pagedResult = await flattenedQuery
                        .Skip(filter.Skip ?? 0)
                        .Take(filter.Take ?? 100)
                        .ToListAsync(cancellationToken);

                    var data = pagedResult.Select(x => new InventorySummaryData
                    {
                        Coin = x.Coin,
                        CurrentAmount = x.CurrentQuantity,
                        SoldAmount = x.SoldQuantity,
                        DateTime = x.LastActivityDate
                    }).ToList();

                    return (data, total);
                }

            case ItemType.Currency:
                {
                    if (!string.IsNullOrEmpty(filter.Search))
                        query = query.Where(x => x.Currency!.Title.Contains(filter.Search));

                    var groupedQuery = query
                        .Where(x => x.CurrencyId != null)
                        .GroupBy(x => x.CurrencyId)
                        .Select(g => new
                        {
                            CurrencyId = g.Key,
                            LastActivityDate = g.Min(x => x.PostingDate),
                            CurrentQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount),
                            SoldQuantity = g.Where(s => s.ActionType == WarehouseActionType.Out &&
                                                        s.InvoiceId != null &&
                                                        s.Invoice!.InvoiceType == InvoiceType.Sell &&
                                                        s.ReverseInventoryStockId == null)
                                            .Sum(s => s.ChangeAmount)
                        });

                    if (inventoryFilter.ActionType == WarehouseActionType.In)
                        groupedQuery = groupedQuery.Where(x => x.CurrentQuantity > 0);
                    else
                        groupedQuery = groupedQuery.Where(x => x.SoldQuantity > 0);

                    var total = await groupedQuery.CountAsync(cancellationToken);

                    var flattenedQuery = groupedQuery.Join(
                        dbContext.Set<PriceUnit>(),
                        g => g.CurrencyId,
                        c => c.Id,
                        (g, c) => new
                        {
                            Currency = c,
                            g.CurrentQuantity,
                            g.SoldQuantity,
                            g.LastActivityDate,
                            CurrencyTitle = c.Title
                        }
                    );

                    var isDesc = filter.SortDirection == null || filter.SortDirection == SortDirection.None ||
                                 filter.SortDirection == SortDirection.Descending;

                    flattenedQuery = filter.SortLabel switch
                    {
                        "Currency.Title" => isDesc ? flattenedQuery.OrderByDescending(x => x.CurrencyTitle) : flattenedQuery.OrderBy(x => x.CurrencyTitle),
                        "CurrentAmount" => isDesc ? flattenedQuery.OrderByDescending(x => x.CurrentQuantity) : flattenedQuery.OrderBy(x => x.CurrentQuantity),
                        "SoldAmount" => isDesc ? flattenedQuery.OrderByDescending(x => x.SoldQuantity) : flattenedQuery.OrderBy(x => x.SoldQuantity),
                        _ => isDesc ? flattenedQuery.OrderByDescending(x => x.CurrencyTitle) : flattenedQuery.OrderBy(x => x.CurrencyTitle)
                    };

                    var pagedResult = await flattenedQuery
                        .Skip(filter.Skip ?? 0)
                        .Take(filter.Take ?? 100)
                        .ToListAsync(cancellationToken);

                    var data = pagedResult.Select(x => new InventorySummaryData
                    {
                        Currency = x.Currency,
                        CurrentAmount = x.CurrentQuantity,
                        SoldAmount = x.SoldQuantity,
                        DateTime = x.LastActivityDate
                    }).ToList();

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
        var productStocks = await dbContext.Set<InventoryStock>()
            .AsNoTracking()
            .Include(x => x.Product!)
            .ThenInclude(p => p.ProductCategory)
            .Where(x => x.ProductId != null && x.Product!.ProductType == calculatorFilter.ProductType)
            .ToListAsync(cancellationToken);

        if (productStocks.Count == 0)
            return ([], 0);

        var groupedByProduct = productStocks
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                g.First().Product,
                CurrentQuantity = g.Sum(s => s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount)
            })
            .Where(x => x.CurrentQuantity > 0)
            .ToList();

        var filtered = groupedByProduct
            .Where(x => string.IsNullOrEmpty(calculatorFilter.Name) || x.Product!.Name.Contains(calculatorFilter.Name))
            .Where(x => !calculatorFilter.Fineness.HasValue || x.Product!.Fineness == calculatorFilter.Fineness.Value)
            .Where(x => !calculatorFilter.MaxWage.HasValue || (x.Product!.WageType == WageType.Percent && x.Product!.Wage <= calculatorFilter.MaxWage.Value))
            .ToList();

        var total = filtered.Count;
        var sorted = filtered.AsQueryable().ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending, "Product.CreatedAt");
        var paged = sorted.Skip(filter.Skip ?? 0).Take(filter.Take ?? 100).ToList();

        var finalData = paged.Select(x => new InventorySummaryData
        {
            Product = x.Product,
            CurrentAmount = x.CurrentQuantity,
            SoldAmount = 0, // در این گزارش صرفاً موجودی فعلی مهم است
            DateTime = x.Product!.CreatedAt
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
    }
}