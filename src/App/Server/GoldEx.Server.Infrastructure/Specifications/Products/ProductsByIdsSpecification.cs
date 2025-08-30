using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByIdsSpecification : SpecificationBase<Product>
{
    public ProductsByIdsSpecification(List<ProductId> productIdsToDelete)
    {
        AddCriteria(x => productIdsToDelete.Contains(x.Id));
    }
}