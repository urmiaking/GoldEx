using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByCategoryPrefixCodeSpecification : SpecificationBase<Product>
{
    public ProductsByCategoryPrefixCodeSpecification(string prefixCode)
    {
        AddInclude(x => x.ProductCategory!);
        AddCriteria(p => p.ProductCategory!.PrefixCode == prefixCode);
    }
}