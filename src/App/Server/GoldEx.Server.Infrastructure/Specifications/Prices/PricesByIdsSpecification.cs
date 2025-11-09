using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesByIdsSpecification : SpecificationBase<Price>
{
    public PricesByIdsSpecification(IEnumerable<PriceId> ids)
    {
        var set = ids.Select(x => x).ToHashSet();
        AddCriteria(p => set.Contains(p.Id));
    }
}