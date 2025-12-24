using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksByFilterSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksByFilterSpecification(RequestFilter filter, InventoryFilter inventoryFilter)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        switch (inventoryFilter.ItemType)
        {
            case ItemType.Product:
                AddInclude(x => x.Product!.ProductCategory!);
                if (!string.IsNullOrEmpty(filter.Search))
                    AddCriteria(x =>
                        x.Product!.Name.Contains(filter.Search) ||
                        x.Product!.Barcode.Contains(filter.Search));
                break;
            case ItemType.Coin:
                AddInclude(x => x.CoinInstance!);
                if (!string.IsNullOrEmpty(filter.Search))
                    AddCriteria(x => x.CoinInstance!.Coin!.Title.Contains(filter.Search) || x.CoinInstance.Barcode == filter.Search);
                break;
            case ItemType.Currency:
                AddInclude(x => x.Currency!);
                if (!string.IsNullOrEmpty(filter.Search))
                    AddCriteria(x =>
                        x.Currency!.Title.Contains(filter.Search));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(InventoryStock.CreatedAt), SortDirection.Descending);
        }

        // Apply date range filter
        if (inventoryFilter.Start.HasValue)
        {
            AddCriteria(x => x.CreatedAt >= inventoryFilter.Start.Value);
        }
        if (inventoryFilter.End.HasValue)
        {
            var endOfDay = inventoryFilter.End.Value.Date.AddDays(1).AddTicks(-1);
            AddCriteria(x => x.CreatedAt <= endOfDay);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}