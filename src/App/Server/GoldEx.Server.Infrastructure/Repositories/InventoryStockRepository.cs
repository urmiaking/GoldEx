using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinInstanceAggregate;
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

    public Task<decimal> GetQuantityAsync(CoinInstanceId coinId, CancellationToken cancellationToken = default)
    {
        return Query
            .Where(stock => stock.CoinInstanceId == coinId)
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

    public async Task<Dictionary<CoinInstanceId, decimal>> GetQuantitiesAsync(IEnumerable<CoinInstanceId> coinIds,
        CancellationToken cancellationToken = default)
    {
        var ids = coinIds.ToList();
        if (!ids.Any())
            return new Dictionary<CoinInstanceId, decimal>();

        return await Query
            .Where(stock => stock.CoinInstanceId != null && ids.Contains(stock.CoinInstanceId.Value))
            .GroupBy(stock => stock.CoinInstanceId!.Value)
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
                        dbContext.Set<Product>().Include(p => p.MoltenGold!.Assayer),
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
                            p.ProductType,
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

                    // --- 7. Details Loading (Sale + Purchase) ---
                    var pagedProductIds = pagedResult.Select(x => x.Product.Id).ToList();

                    // Fetch raw details for both Sell and Purchase invoices
                    var rawDetails = await dbContext.Set<Invoice>()
                        .AsNoTracking()
                        .Where(i => i.InvoiceType == InvoiceType.Sell || i.InvoiceType == InvoiceType.Purchase)
                        .SelectMany(i => i.ProductItems.Select(pi => new
                        {
                            pi.ProductId,
                            i.InvoiceType,
                            Date = i.InvoiceDate,
                            ItemId = pi.Id,

                            // Sale Properties
                            pi.SaleWage,
                            pi.SaleWageType,
                            pi.SaleWagePriceUnitId,

                            // Purchase Properties
                            pi.PurchaseWage,
                            pi.PurchaseWageType,
                            pi.PurchaseWagePriceUnitId
                        }))
                        .Where(x => pagedProductIds.Contains(x.ProductId))
                        .ToListAsync(cancellationToken);

                    // Group in memory to split Sale vs Purchase
                    var productDetails = rawDetails
                        .GroupBy(x => x.ProductId)
                        .ToDictionary(
                            g => g.Key,
                            g => new
                            {
                                LatestSale = g.Where(x => x.InvoiceType == InvoiceType.Sell)
                                              .OrderByDescending(x => x.Date)
                                              .ThenByDescending(x => x.ItemId.Value)
                                              .FirstOrDefault(),

                                LatestPurchase = g.Where(x => x.InvoiceType == InvoiceType.Purchase)
                                                  .OrderByDescending(x => x.Date)
                                                  .ThenByDescending(x => x.ItemId.Value)
                                                  .FirstOrDefault()
                            }
                        );

                    // Collect all PriceUnitIds (Sale + Purchase) for fetching titles
                    var unitIds = productDetails.Values
                        .SelectMany(v => new[] { v.LatestSale?.SaleWagePriceUnitId, v.LatestPurchase?.PurchaseWagePriceUnitId })
                        .Where(id => id.HasValue)
                        .Select(id => id!.Value)
                        .Distinct()
                        .ToList();

                    Dictionary<PriceUnitId, string> priceUnitTitles = [];
                    if (unitIds.Any())
                    {
                        priceUnitTitles = await dbContext.Set<PriceUnit>()
                            .AsNoTracking()
                            .Where(u => unitIds.Contains(u.Id))
                            .ToDictionaryAsync(u => u.Id, u => u.Title, cancellationToken);
                    }

                    // --- 10. Final Projection ---
                    var finalData = pagedResult.Select(x =>
                    {
                        var details = productDetails.GetValueOrDefault(x.Product.Id);
                        var sale = details?.LatestSale;
                        var purchase = details?.LatestPurchase;

                        // Resolve Titles
                        string? saleWageTitle = null;
                        if (sale?.SaleWagePriceUnitId != null)
                            priceUnitTitles.TryGetValue(sale.SaleWagePriceUnitId.Value, out saleWageTitle);

                        string? purchaseWageTitle = null;
                        if (purchase?.PurchaseWagePriceUnitId != null)
                            priceUnitTitles.TryGetValue(purchase.PurchaseWagePriceUnitId.Value, out purchaseWageTitle);

                        return new InventorySummaryData
                        {
                            Product = x.Product,
                            CurrentAmount = x.CurrentQuantity,
                            SoldAmount = x.SoldQuantity,
                            DateTime = x.LastActivityDate,

                            // Sale
                            SaleWage = sale?.SaleWage,
                            SaleWageType = sale?.SaleWageType,
                            SaleWagePriceUnitTitle = saleWageTitle,

                            // Purchase
                            PurchaseWage = purchase?.PurchaseWage,
                            PurchaseWageType = purchase?.PurchaseWageType,
                            PurchaseWagePriceUnitTitle = purchaseWageTitle
                        };
                    }).ToList();

                    return (finalData, total);
                }

            case ItemType.Coin:
                {
                    query = query.Where(x => x.CoinInstanceId != null);

                    if (!string.IsNullOrEmpty(filter.Search))
                    {
                        query = query.Where(x =>
                            x.CoinInstance!.Barcode.Contains(filter.Search));
                    }

                    var groupedQuery = query
                        .GroupBy(x => x.CoinInstanceId)
                        .Select(g => new
                        {
                            CoinInstanceId = g.Key!,
                            LastActivityDate = g.Min(x => x.PostingDate),

                            CurrentQuantity = g.Sum(s =>
                                s.ActionType == WarehouseActionType.In
                                    ? s.ChangeAmount
                                    : -s.ChangeAmount
                            ),

                            SoldQuantity = g
                                .Where(s =>
                                    s.ActionType == WarehouseActionType.Out &&
                                    s.InvoiceId != null &&
                                    s.Invoice!.InvoiceType == InvoiceType.Sell &&
                                    s.ReverseInventoryStockId == null
                                )
                                .Sum(s => s.ChangeAmount)
                        });

                    if (inventoryFilter.ActionType == WarehouseActionType.In)
                        groupedQuery = groupedQuery.Where(x => x.CurrentQuantity > 0);
                    else
                        groupedQuery = groupedQuery.Where(x => x.SoldQuantity > 0);

                    var total = await groupedQuery.CountAsync(cancellationToken);

                    var flattenedQuery = groupedQuery.Join(
                        dbContext.Set<CoinInstance>()
                            .AsNoTracking()
                            .Include(ci => ci.Coin)
                            .Include(ci => ci.CoinInstancePackage!.Issuer),
                        g => g.CoinInstanceId,
                        ci => ci.Id,
                        (g, ci) => new
                        {
                            CoinInstance = ci,
                            g.CurrentQuantity,
                            g.SoldQuantity,
                            g.LastActivityDate,
                            ci.Barcode,
                            ci.MintYear,
                            ci.PackageType
                        });

                    var isDesc =
                        filter.SortDirection == null ||
                        filter.SortDirection == SortDirection.None ||
                        filter.SortDirection == SortDirection.Descending;

                    flattenedQuery = filter.SortLabel switch
                    {
                        "Coin.Barcode" =>
                            isDesc
                                ? flattenedQuery.OrderByDescending(x => x.Barcode)
                                : flattenedQuery.OrderBy(x => x.Barcode),

                        "Coin.Weight" =>
                            isDesc
                                ? flattenedQuery.OrderByDescending(x => x.CoinInstance.Weight)
                                : flattenedQuery.OrderBy(x => x.CoinInstance.Weight),

                        "Coin.Fineness" =>
                            isDesc
                                ? flattenedQuery.OrderByDescending(x => x.CoinInstance.Fineness)
                                : flattenedQuery.OrderBy(x => x.CoinInstance.Fineness),

                        "Coin.Title" =>
                            isDesc
                                ? flattenedQuery.OrderByDescending(x => x.CoinInstance.Coin!.Title)
                                : flattenedQuery.OrderBy(x => x.CoinInstance.Coin!.Title),

                        "Coin.MintYear" =>
                            isDesc
                                ? flattenedQuery.OrderByDescending(x => x.MintYear)
                                : flattenedQuery.OrderBy(x => x.MintYear),

                        "CurrentAmount" =>
                            isDesc
                                ? flattenedQuery.OrderByDescending(x => x.CurrentQuantity)
                                : flattenedQuery.OrderBy(x => x.CurrentQuantity),

                        "SoldAmount" =>
                            isDesc
                                ? flattenedQuery.OrderByDescending(x => x.SoldQuantity)
                                : flattenedQuery.OrderBy(x => x.SoldQuantity),

                        _ =>
                            isDesc
                                ? flattenedQuery.OrderByDescending(x => x.LastActivityDate)
                                : flattenedQuery.OrderBy(x => x.LastActivityDate)
                    };

                    var pagedResult = await flattenedQuery
                        .Skip(filter.Skip ?? 0)
                        .Take(filter.Take ?? 100)
                        .ToListAsync(cancellationToken);

                    var data = pagedResult.Select(x => new InventorySummaryData
                    {
                        CoinInstance = x.CoinInstance,
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

    public async Task<List<InventoryWeightChartData>> GetInventoryWeightChartDataAsync(WarehouseActionType actionType, CancellationToken cancellationToken = default)
    {
        // ---------- Base Query ----------
        var stocksQuery = dbContext.Set<InventoryStock>()
            .AsNoTracking()
            .Include(x => x.Product)
                .ThenInclude(p => p!.ProductCategory)
            .Include(x => x.MoltenGoldDetail)
            .Include(x => x.CoinInstance)
                .ThenInclude(ci => ci!.Coin)
            .Include(x => x.Invoice)
            .AsQueryable();

        // ---------- Filter by ActionType ----------
        if (actionType == WarehouseActionType.In)
        {
            // موجودی
            stocksQuery = stocksQuery.Where(x =>
                x.ActionType == WarehouseActionType.In ||
                x.ActionType == WarehouseActionType.Out);
        }
        else
        {
            // فروخته‌شده واقعی
            stocksQuery = stocksQuery.Where(x =>
                x.ActionType == WarehouseActionType.Out &&
                x.InvoiceId != null &&
                x.Invoice!.InvoiceType == InvoiceType.Sell &&
                x.ReverseInventoryStockId == null);
        }

        var stocks = await stocksQuery.ToListAsync(cancellationToken);

        if (!stocks.Any())
            return [];

        var result = new List<InventoryWeightChartData>();

        // =====================================================
        // 1. PRODUCTS (Jewelry + Gold + UsedGold)
        // Grouped by ProductCategory
        // =====================================================

        var productWeights = stocks
            .Where(x => x.ProductId != null &&
                        x.Product!.ProductType != ProductType.MoltenGold)
            .GroupBy(x => x.Product!.ProductCategory?.Title ?? "بدون دسته‌بندی")
            .Select(g => new InventoryWeightChartData(
                g.Key,
                g.Sum(s =>
                    s.ActionType == actionType
                        ? s.ChangeAmount
                        : -s.ChangeAmount
                )
            ))
            .Where(x => x.Weight > 0)
            .ToList();

        result.AddRange(productWeights);

        // =====================================================
        // 2. MOLTEN GOLD
        // =====================================================
        var moltenGoldWeight = stocks
            .Where(x => x.Product?.ProductType == ProductType.MoltenGold)
            .Sum(s =>
                s.ActionType == actionType
                    ? s.ChangeAmount
                    : -s.ChangeAmount
            );

        if (moltenGoldWeight > 0)
        {
            result.Add(new InventoryWeightChartData(
                ProductType.MoltenGold.GetDisplayName(),
                moltenGoldWeight
            ));
        }

        // =====================================================
        // 3. COINS
        // Grouped by Coin (not CoinInstance)
        // =====================================================

        var coinData = stocks
            .Where(x => x.CoinInstanceId != null)
            .GroupBy(x => x.CoinInstanceId)
            .Select(g =>
            {
                var coin = g.First().CoinInstance!.Coin!;
                var quantity = g.Sum(s =>
                    s.ActionType == actionType
                        ? s.ChangeAmount
                        : -s.ChangeAmount);

                return new
                {
                    Coin = coin,
                    Quantity = quantity
                };
            })
            .Where(x => x.Quantity > 0)
            .GroupBy(x => x.Coin.Title)
            .Select(g => new InventoryWeightChartData(
                g.Key,
                g.Sum(c => c.Quantity * c.Coin.Weight)
            ))
            .ToList();

        result.AddRange(coinData);

        result = result
            .Select(x => x with { Weight = Math.Round(x.Weight, 0, MidpointRounding.AwayFromZero) })
            .ToList();

        return result;
    }
}