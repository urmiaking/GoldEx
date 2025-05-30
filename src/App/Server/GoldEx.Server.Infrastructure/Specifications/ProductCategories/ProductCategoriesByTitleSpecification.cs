using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductCategories;

public class ProductCategoriesByTitleSpecification : SpecificationBase<ProductCategory>
{
    public ProductCategoriesByTitleSpecification(string title)
    {
        AddCriteria(x => x.Title == title);
    }
}