using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductCategories;

public class ProductCategoriesByIdSpecification : SpecificationBase<ProductCategory>
{
    public ProductCategoriesByIdSpecification(ProductCategoryId id)
    {
        AddCriteria(x => x.Id == id);
    }
}