using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceUnits;

public class PriceUnitsDefaultSpecification : SpecificationBase<PriceUnit>
{
    public PriceUnitsDefaultSpecification()
    {
        AddCriteria(x => x.IsActive);
        ApplyOrderBy(x => x.Title);
    }
}