using GoldEx.Sdk.Common.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductCategories;

public class ProductCategoriesByLooseMatchSpecification
    : SpecificationBase<ProductCategory>
{
    public ProductCategoriesByLooseMatchSpecification(string normalizedTitle)
    {
        var variants = normalizedTitle.ExpandVariants();

        AddCriteria(x => variants.Contains(x.Title.Replace("ي", "ی")
            .Replace("ى", "ی")
            .Replace("ئ", "ی")
            .Replace("ك", "ک")
            .Replace("\u200C", "")));
    }
}