using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesDefaultSpecification : SpecificationBase<Price>
{
    public PricesDefaultSpecification(bool? isPinned = false)
    {
        AddInclude(x => x.PriceUnit!);

        AddCriteria(x => x.IsActive);

        if (isPinned.HasValue)
        {
            AddCriteria(x => x.IsPinned == isPinned.Value);
        }

        ApplyOrderBy(x => x.MarketType);
        ApplyOrderBy(x => x.Title);
    }
}