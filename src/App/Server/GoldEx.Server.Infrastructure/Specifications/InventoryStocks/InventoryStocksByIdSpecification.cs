using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksByIdSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksByIdSpecification(InventoryStockId id)
    {
        AddCriteria(x => x.Id == id);
    }
}