using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductCategories;

public class ProductCategoriesDefaultSpecification : SpecificationBase<ProductCategory>
{
    public ProductCategoriesDefaultSpecification()
    {
        ApplyOrderBy(x => x.Title);
    }
}