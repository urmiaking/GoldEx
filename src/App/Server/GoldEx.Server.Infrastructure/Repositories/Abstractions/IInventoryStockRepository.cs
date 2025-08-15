using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IInventoryStockRepository : IRepository<InventoryStock>,
    ICreateRepository<InventoryStock>,
    IDeleteRepository<InventoryStock>
{
    Task<decimal> GetQuantityAsync(ProductId productId, CancellationToken cancellationToken = default);
    Task<decimal> GetQuantityAsync(CoinId coinId, CancellationToken cancellationToken = default);
    Task<decimal> GetQuantityAsync(PriceUnitId currencyId, CancellationToken cancellationToken = default);

    Task<Dictionary<ProductId, decimal>> GetQuantitiesAsync(IEnumerable<ProductId> productIds, CancellationToken cancellationToken = default);
    Task<Dictionary<CoinId, decimal>> GetQuantitiesAsync(IEnumerable<CoinId> coinIds, CancellationToken cancellationToken = default);
    Task<Dictionary<PriceUnitId, decimal>> GetQuantitiesAsync(IEnumerable<PriceUnitId> currencyIds, CancellationToken cancellationToken = default);
}