using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
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
}