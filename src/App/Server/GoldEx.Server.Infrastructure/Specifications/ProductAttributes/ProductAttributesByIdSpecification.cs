using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAttributeAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductAttributes;

public class ProductAttributesByIdSpecification : SpecificationBase<ProductAttribute>
{
    public ProductAttributesByIdSpecification(ProductAttributeId id)
    {
        AddCriteria(x => x.Id == id);
    }
}
