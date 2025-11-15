using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryEntryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryEntries;

public class InventoryEntriesByIdSpecification : SpecificationBase<InventoryEntry>
{
    public InventoryEntriesByIdSpecification(InventoryEntryId id)
    {
        AddCriteria(x => x.Id == id);
    }
}