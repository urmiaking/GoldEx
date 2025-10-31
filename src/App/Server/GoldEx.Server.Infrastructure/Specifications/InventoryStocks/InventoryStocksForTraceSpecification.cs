using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksForTraceSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksForTraceSpecification(Guid itemId, ItemType itemType, RequestFilter filter)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        AddInclude(x => x.Currency!);
        AddInclude(x => x.Coin!);
        AddInclude(x => x.Product!);
        AddInclude(x => x.MeltingBatch!);
        AddInclude(x => x.Invoice!.Customer!);

        switch (itemType)
        {
            case ItemType.Product:
            case ItemType.MoltenGold:
            case ItemType.UsedProduct:
                AddCriteria(x => x.ProductId == new ProductId(itemId));
                break;
            case ItemType.Coin:
                AddCriteria(x => x.CoinId == new CoinId(itemId));
                break;
            case ItemType.Currency:
                AddCriteria(x => x.CurrencyId == new PriceUnitId(itemId));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(InventoryStock.PostingDate), SortDirection.Ascending);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}