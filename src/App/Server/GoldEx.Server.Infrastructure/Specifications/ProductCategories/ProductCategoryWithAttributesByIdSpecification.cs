using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductCategories;

public class ProductCategoryWithAttributesByIdSpecification : SpecificationBase<ProductCategory>
{
    public ProductCategoryWithAttributesByIdSpecification(ProductCategoryId id)
    {
        AddCriteria(x => x.Id == id);
        AddInclude(x => x.Attributes);
    }
}
