using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesByMarketTypeSpecification : SpecificationBase<Price>
{
    public PricesByMarketTypeSpecification(MarketType marketType)
    {
        AddCriteria(x => x.MarketType == marketType);
        ApplyOrderBy(x => x.Title);
    }
}