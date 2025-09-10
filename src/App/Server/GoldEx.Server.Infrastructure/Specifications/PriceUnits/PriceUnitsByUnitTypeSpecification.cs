using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.PriceUnits;

public class PriceUnitsByUnitTypeSpecification : SpecificationBase<PriceUnit>
{
    public PriceUnitsByUnitTypeSpecification(UnitType unitType)
    {
        AddCriteria(x => x.UnitType == unitType);
    }
}