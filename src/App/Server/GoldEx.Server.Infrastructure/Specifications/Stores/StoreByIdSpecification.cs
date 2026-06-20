using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoreByIdSpecification : SpecificationBase<Store>
{
    public StoreByIdSpecification(StoreId storeId)
    {
        AddCriteria(x => x.Id == storeId && x.IsActive);
    }
}
