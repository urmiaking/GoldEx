using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoreUserByStoreIdSpecification : SpecificationBase<StoreUser>
{
    public StoreUserByStoreIdSpecification(StoreId storeId)
    {
        AddCriteria(x => x.StoreId == storeId);
    }
}
