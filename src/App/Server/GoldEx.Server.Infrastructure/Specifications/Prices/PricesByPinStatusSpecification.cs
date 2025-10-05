using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesByPinStatusSpecification : SpecificationBase<Price>
{
    public PricesByPinStatusSpecification()
    {
        AddCriteria(x => x.IsPinned);
    }
}