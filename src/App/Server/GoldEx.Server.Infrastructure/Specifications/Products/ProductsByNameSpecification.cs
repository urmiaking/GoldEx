using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByNameSpecification : SpecificationBase<Product>
{
    public ProductsByNameSpecification(string name)
    {
        AddCriteria(x => x.Name.Contains(name));

        AddInclude(x => x.InventoryStocks!);
        AddInclude(x => x.ProductCategory!);
        AddInclude(x => x.WagePriceUnit!);
        AddInclude(x => x.StonePriceUnit!);
    }
}