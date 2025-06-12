using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByIdSpecification : SpecificationBase<Product>
{
    public ProductsByIdSpecification(ProductId id)
    {
        AddCriteria(x => x.Id == id);

        AddInclude(x => x.WagePriceUnit!);
    }
}