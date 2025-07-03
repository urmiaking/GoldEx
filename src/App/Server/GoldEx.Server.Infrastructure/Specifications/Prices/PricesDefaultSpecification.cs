using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesDefaultSpecification : SpecificationBase<Price>
{
    public PricesDefaultSpecification()
    {
        AddCriteria(x => x.IsActive);
        ApplyOrderBy(x => x.MarketType);
        ApplyOrderBy(x => x.Title);
    }
}