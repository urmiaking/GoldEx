using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksByCoinIdSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksByCoinIdSpecification(CoinId coinId)
    {
        AddCriteria(x => x.CoinId == coinId);
    }
}