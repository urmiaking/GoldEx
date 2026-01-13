using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksKardexSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksKardexSpecification(ProductId? productId, CoinInstanceId? coinInstanceId, PriceUnitId? currencyId)
    {
        AddInclude(x => x.Currency!);
        AddInclude(x => x.CoinInstance!);
        AddInclude(x => x.Product!);
        AddInclude(x => x.MeltingBatch!);
        AddInclude(x => x.Invoice!.Customer!);
        AddInclude(x => x.Transactions!);

        if (productId.HasValue)
        {
            AddCriteria(x => x.ProductId == productId);
        }
        else if (coinInstanceId.HasValue)
        {
            AddCriteria(x => x.CoinInstanceId == coinInstanceId);
        }
        else if (currencyId.HasValue)
        {
            AddCriteria(x => x.CurrencyId == currencyId);
        }
    }
}