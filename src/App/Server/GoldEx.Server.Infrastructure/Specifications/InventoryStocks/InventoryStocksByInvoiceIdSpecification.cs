using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksByInvoiceIdSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksByInvoiceIdSpecification(InvoiceId invoiceId, ItemType? itemType = null)
    {
        AddCriteria(x => x.InvoiceId == invoiceId);

        if (itemType.HasValue)
        {
            switch (itemType.Value)
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
    }
}