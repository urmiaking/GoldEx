using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceUnits;

public class PriceUnitsSetAsDefaultSpecification : SpecificationBase<PriceUnit>
{
    public PriceUnitsSetAsDefaultSpecification()
    {
        AddCriteria(x => x.IsDefault);
    }
}