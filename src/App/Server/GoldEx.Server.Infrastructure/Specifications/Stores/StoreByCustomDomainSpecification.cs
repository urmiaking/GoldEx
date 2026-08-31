using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoreByCustomDomainSpecification : SpecificationBase<Store>
{
    public StoreByCustomDomainSpecification(string domain)
    {
        var cleanDomain = domain.ToLowerInvariant().Trim();
        AddCriteria(x => x.CustomDomain != null && x.CustomDomain.ToLower() == cleanDomain);
    }
}
