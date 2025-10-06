using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductCategories;

public class ProductCategoriesByPrefixCodeSpecification : SpecificationBase<ProductCategory>
{
    public ProductCategoriesByPrefixCodeSpecification(string prefixCode)
    {
        AddCriteria(x => x.PrefixCode == prefixCode);
    }
}