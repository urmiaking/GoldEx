using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceUnits;

public class PriceUnitsByIdSpecification : SpecificationBase<PriceUnit>
{
    public PriceUnitsByIdSpecification(PriceUnitId id)
    {
        AddCriteria(x => x.Id == id);
    }
}