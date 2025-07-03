using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceUnits;

public class PriceUnitsWithoutPriceIdSpecification : SpecificationBase<PriceUnit>
{
    public PriceUnitsWithoutPriceIdSpecification()
    {
        AddCriteria(x => !x.PriceId.HasValue);
    }
}