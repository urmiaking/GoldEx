using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.PriceUnits;

public class PriceUnitsByTitleSpecification : SpecificationBase<PriceUnit>
{
    public PriceUnitsByTitleSpecification(string title)
    {
        AddCriteria(x => x.Title == title);
    }
}