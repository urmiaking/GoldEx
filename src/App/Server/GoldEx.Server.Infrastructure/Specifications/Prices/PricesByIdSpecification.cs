using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesByIdSpecification : SpecificationBase<Price>
{
    public PricesByIdSpecification(PriceId id)
    {
        AddCriteria(x => x.Id == id);
    }
}