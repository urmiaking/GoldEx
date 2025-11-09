using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Prices;

public sealed class PricesByPriceCatalogSpecification : SpecificationBase<Price>
{
    public PricesByPriceCatalogSpecification(PriceCatalog priceCatalog)
    {
        AddCriteria(x => x.PriceCatalog == priceCatalog);
    }
}