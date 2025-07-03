using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceUnits;

public class PriceUnitsWithoutSpecification : SpecificationBase<PriceUnit>
{
    public PriceUnitsWithoutSpecification()
    {
        ApplyOrderBy(x => x.Title);
    }
}