using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksByProductIdSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksByProductIdSpecification(ProductId productId)
    {
        AddCriteria(x => x.ProductId == productId);
        AddInclude(x => x.Product!);
        AddInclude(x => x.Transactions!);
    }
}