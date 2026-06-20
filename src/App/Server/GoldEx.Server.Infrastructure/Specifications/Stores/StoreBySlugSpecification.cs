using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoreBySlugSpecification : SpecificationBase<Store>
{
    public StoreBySlugSpecification(string slug)
    {
        AddCriteria(x => x.Slug == slug.ToLowerInvariant().Trim());
    }
}
