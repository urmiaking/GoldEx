using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryExitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryExits;

public class InventoryExitsByIdSpecification : SpecificationBase<InventoryExit>
{
    public InventoryExitsByIdSpecification(InventoryExitId id)
    {
        AddCriteria(x => x.Id == id);
    }
}