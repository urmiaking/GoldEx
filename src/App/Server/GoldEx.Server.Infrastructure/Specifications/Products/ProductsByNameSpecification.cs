using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByNameSpecification : SpecificationBase<Product>
{
    public ProductsByNameSpecification(string name, ProductType? productType = null)
    {
        AddCriteria(x => x.Name.Contains(name));
        AddCriteria(x => x.ProductType != ProductType.UsedGold);

        if (productType.HasValue)
            AddCriteria(x => x.ProductType == productType.Value);

        AddInclude(x => x.InventoryStocks!);
        AddInclude(x => x.ProductCategory!);
        AddInclude(x => x.WagePriceUnit!);
        AddInclude(x => x.StonePriceUnit!);
        AddInclude(x => x.MoltenGold!);
        AddInclude(x => x.MoltenGold!.Assayer!);

        ApplyOrderByDescending(x => x.CreatedAt);
    }
}