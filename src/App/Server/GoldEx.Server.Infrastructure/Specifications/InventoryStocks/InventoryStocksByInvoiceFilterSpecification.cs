using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryStocks;

public class InventoryStocksByInvoiceFilterSpecification : SpecificationBase<InventoryStock>
{
    public InventoryStocksByInvoiceFilterSpecification(InvoiceId invoiceId, RequestFilter filter)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        AddInclude(x => x.Coin!);
        AddInclude(x => x.Currency!);
        AddInclude(x => x.Product!);

        AddCriteria(x => x.InvoiceId == invoiceId);

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