using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByMoltenGoldSpecification : SpecificationBase<Product>
{
    public ProductsByMoltenGoldSpecification(decimal fineness)
    {
        AddCriteria(x => x.ProductType == ProductType.MoltenGold && x.Fineness == fineness);
    }
}