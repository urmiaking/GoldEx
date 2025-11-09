using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceProviderMappingAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceProviderMappings;

public class PriceProviderMappingsDefaultSpecification : SpecificationBase<PriceProviderMapping>
{
    public PriceProviderMappingsDefaultSpecification(bool? isEnabled = true)
    {
        if (isEnabled.HasValue)
        {
            AddCriteria(x => x.IsEnabled == isEnabled);
        }
    }
}