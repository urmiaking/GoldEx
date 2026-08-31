using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class ActiveStoresSpecification : SpecificationBase<Store>
{
    public ActiveStoresSpecification()
    {
        AddCriteria(x => x.IsActive);
    }
}
