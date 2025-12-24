using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksByItemTypeSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksByItemTypeSpecification(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Product:
                AddCriteria(x => x.ProductId.HasValue);
                break;
            case ItemType.Coin:
                AddCriteria(x => x.CoinInstanceId.HasValue);
                break;
            case ItemType.Currency:
                AddCriteria(x => x.CurrencyId.HasValue);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }
    }

    public InventoryStocksByItemTypeSpecification(ProductType productType)
    {
        if (productType is ProductType.UsedGold)
            throw new InvalidOperationException($"{ProductType.UsedGold.ToString()} is not used for database operations but for UI purposes");

        AddInclude(x => x.Product!);

        AddCriteria(x => x.ProductId.HasValue && x.Product!.ProductType == productType);
    }
}