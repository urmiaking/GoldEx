using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsForVitrineSpecification : SpecificationBase<Product>
{
    public ProductsForVitrineSpecification(Guid? storeId = null)
    {
        if (storeId.HasValue)
        {
            AddCriteria(x => x.StoreId == new StoreId(storeId.Value));
        }

        AddCriteria(x => x.ShowInVitrine && (x.ProductType == ProductType.Gold || x.ProductType == ProductType.Jewelry));

        AddInclude(x => x.ProductCategory!);
        AddInclude(x => x.WagePriceUnit!);
        AddInclude(x => x.StonePriceUnit!);
        AddInclude(x => x.Images);
        AddInclude(x => x.GemStones);
    }
}
