using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStockOriginSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStockOriginSpecification(ProductId productId)
    {
        AddCriteria(x => x.ProductId == productId && x.InventoryEntryId != null);
    }
}