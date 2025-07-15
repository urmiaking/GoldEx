using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByCategoryIdSpecification : SpecificationBase<Product>
{
    public ProductsByCategoryIdSpecification(ProductCategoryId id)
    {
        AddCriteria(x => x.ProductCategoryId == id);
        AddInclude(x => x.InvoiceItem!);
    }
}