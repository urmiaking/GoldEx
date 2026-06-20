using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;
using System.Collections.Generic;
using System.Linq;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoresByIdsSpecification : SpecificationBase<Store>
{
    public StoresByIdsSpecification(IEnumerable<StoreId> ids)
    {
        AddCriteria(x => ids.Contains(x.Id) && x.IsActive);
    }
}
