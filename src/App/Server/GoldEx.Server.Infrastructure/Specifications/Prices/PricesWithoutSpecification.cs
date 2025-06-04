using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesWithoutSpecification : SpecificationBase<Price>
{
    public PricesWithoutSpecification()
    {
        ApplyOrderBy(x => x.MarketType);
        ApplyOrderBy(x => x.Title);
    }
}