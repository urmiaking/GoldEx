using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ProductCategories;

public class ProductCategoriesByStoreIdSpecification : SpecificationBase<ProductCategory>
{
    public ProductCategoriesByStoreIdSpecification(StoreId storeId)
    {
        AddCriteria(x => x.StoreId == storeId);
        ApplyOrderBy(x => x.PrefixCode);
    }
}
