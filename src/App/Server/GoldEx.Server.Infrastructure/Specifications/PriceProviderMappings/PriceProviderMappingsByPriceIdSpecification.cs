using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceProviderMappingAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceProviderMappings;

public class PriceProviderMappingsByPriceIdSpecification : SpecificationBase<PriceProviderMapping>
{
    public PriceProviderMappingsByPriceIdSpecification(PriceId priceId)
    {
        AddCriteria(x => x.PriceId == priceId);
    }
}