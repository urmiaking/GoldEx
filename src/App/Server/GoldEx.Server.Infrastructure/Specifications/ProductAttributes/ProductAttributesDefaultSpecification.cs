using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAttributeAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductAttributes;

public class ProductAttributesDefaultSpecification : SpecificationBase<ProductAttribute>
{
    public ProductAttributesDefaultSpecification()
    {
        ApplyOrderBy(x => x.Title);
    }
}
