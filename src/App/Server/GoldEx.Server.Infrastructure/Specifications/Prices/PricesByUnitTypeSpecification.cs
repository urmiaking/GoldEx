using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public class PricesByUnitTypeSpecification : SpecificationBase<Price>
{
    public PricesByUnitTypeSpecification(UnitType unitType)
    {
        AddCriteria(x => x.UnitType == unitType);
    }
}