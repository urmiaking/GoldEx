using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoreByIdAnyStatusSpecification : SpecificationBase<Store>
{
    public StoreByIdAnyStatusSpecification(StoreId storeId)
    {
        AddCriteria(x => x.Id == storeId);
    }
}
