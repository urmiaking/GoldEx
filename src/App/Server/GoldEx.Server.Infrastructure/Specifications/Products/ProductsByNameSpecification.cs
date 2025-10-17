using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByNameSpecification : SpecificationBase<Product>
{
    public ProductsByNameSpecification(string name)
    {
        AddCriteria(x => x.Name.Contains(name));
        AddCriteria(x => x.ProductType != ProductType.UsedGold);

        AddInclude(x => x.InventoryStocks!);
        AddInclude(x => x.ProductCategory!);
        AddInclude(x => x.WagePriceUnit!);
        AddInclude(x => x.StonePriceUnit!);
    }
}