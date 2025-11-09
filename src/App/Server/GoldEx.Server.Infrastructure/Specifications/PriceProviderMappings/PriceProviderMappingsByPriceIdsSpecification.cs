using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceProviderMappingAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceProviderMappings;

public class PriceProviderMappingsByPriceIdsSpecification : SpecificationBase<PriceProviderMapping>
{
    public PriceProviderMappingsByPriceIdsSpecification(IEnumerable<PriceId> priceIds)
    {
        var idSet = priceIds.Select(x => x).ToHashSet();
        AddCriteria(x => idSet.Contains(x.PriceId));
    }
}