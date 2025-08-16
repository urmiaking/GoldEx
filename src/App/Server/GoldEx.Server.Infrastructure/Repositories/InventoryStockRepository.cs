using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
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

    public async Task<(List<InventorySummaryData> Data, int Total)> GetInventorySummaryAsync(RequestFilter filter, InventoryFilter inventoryFilter,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = Query
            .Where(x => !inventoryFilter.Start.HasValue || x.CreatedAt >= inventoryFilter.Start.Value)
            .Where(x => !inventoryFilter.End.HasValue || x.CreatedAt <= inventoryFilter.End.Value);

        switch (inventoryFilter.ItemType)
        {
            case ItemType.Product:
                var productQuery = baseQuery
                    .Where(x => x.ProductId != null)
                    .GroupBy(x => x.Product)
                    .Select(g => new { Item = g.Key, Quantity = g.Sum(s =>
                        s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount) });

                if (!string.IsNullOrEmpty(filter.Search))
                    productQuery = productQuery.Where(x => x.Item!.Name.Contains(filter.Search) || x.Item!.Barcode.Contains(filter.Search));

                var productTotal = await productQuery.CountAsync(cancellationToken);
                var productSorted = productQuery.ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending, 
                    "Product.CreatedAt");

                var productData = await productSorted
                    .Skip(filter.Skip ?? 0).Take(filter.Take ?? 100)
                    .Select(x => new InventorySummaryData { Product = x.Item, CurrentQuantity = x.Quantity })
                    .ToListAsync(cancellationToken);

                return (productData, productTotal);

            case ItemType.Coin:
                var coinQuery = baseQuery
                    .Where(x => x.CoinId != null)
                    .GroupBy(x => x.Coin)
                    .Select(g => new { Item = g.Key, Quantity = g.Sum(s =>
                        s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount) });

                if (!string.IsNullOrEmpty(filter.Search))
                    coinQuery = coinQuery.Where(x => x.Item!.Title.Contains(filter.Search));

                var coinTotal = await coinQuery.CountAsync(cancellationToken);
                var coinSorted = coinQuery.ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending,
                    "Coin.Title");
                var coinData = await coinSorted
                    .Skip(filter.Skip ?? 0).Take(filter.Take ?? 100)
                    .Select(x => new InventorySummaryData { Coin = x.Item, CurrentQuantity = x.Quantity })
                    .ToListAsync(cancellationToken);
                return (coinData, coinTotal);

            case ItemType.Currency:
                var currencyQuery = baseQuery
                    .Where(x => x.CurrencyId != null)
                    .GroupBy(x => x.Currency)
                    .Select(g => new { Item = g.Key, Quantity = g.Sum(s =>
                        s.ActionType == WarehouseActionType.In ? s.ChangeAmount : -s.ChangeAmount) });

                if (!string.IsNullOrEmpty(filter.Search))
                    currencyQuery = currencyQuery.Where(x => x.Item!.Title.Contains(filter.Search));

                var currencyTotal = await currencyQuery.CountAsync(cancellationToken);
                var currencySorted = currencyQuery.ApplySorting(filter.SortLabel, filter.SortDirection ?? SortDirection.Descending,
                    "Currency.Title");
                var currencyData = await currencySorted
                    .Skip(filter.Skip ?? 0).Take(filter.Take ?? 100)
                    .Select(x => new InventorySummaryData { Currency = x.Item, CurrentQuantity = x.Quantity })
                    .ToListAsync(cancellationToken);
                return (currencyData, currencyTotal);

            default:
                throw new ArgumentOutOfRangeException(nameof(inventoryFilter.ItemType), "Invalid item type for inventory summary.");
        }

    }
}