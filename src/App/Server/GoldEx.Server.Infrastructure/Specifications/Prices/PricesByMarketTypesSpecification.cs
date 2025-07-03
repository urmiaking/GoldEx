using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesByMarketTypesSpecification : SpecificationBase<Price>
{
    public PricesByMarketTypesSpecification(MarketType[] marketTypes)
    {
        AddCriteria(x => marketTypes.Contains(x.MarketType));
    }
}