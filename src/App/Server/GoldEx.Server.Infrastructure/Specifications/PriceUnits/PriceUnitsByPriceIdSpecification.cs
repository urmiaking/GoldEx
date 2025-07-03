using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceUnits;

public class PriceUnitsByPriceIdSpecification : SpecificationBase<PriceUnit>
{
    public PriceUnitsByPriceIdSpecification(PriceId priceId)
    {
        AddCriteria(x => x.PriceId == priceId);
    }
}