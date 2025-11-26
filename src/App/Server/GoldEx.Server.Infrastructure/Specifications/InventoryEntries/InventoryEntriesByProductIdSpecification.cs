using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryEntries;

public class InventoryEntriesByProductIdSpecification : SpecificationBase<InventoryEntry>
{
    public InventoryEntriesByProductIdSpecification(ProductId productId)
    {
        AddCriteria(x => x.InventoryStocks!.Any(y => y.ProductId == productId));
    }
}