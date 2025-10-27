using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByFilterSpecification : SpecificationBase<Transaction>
{
    public TransactionsByFilterSpecification(TransactionFilter transactionFilter, RequestFilter filter)
    {
        // Includes
        AddInclude(x => x.PriceUnit!);
        AddInclude(x => x.LedgerAccount!);

        // Apply filter
        if (transactionFilter.Start.HasValue)
        {
            AddCriteria(x => x.CreatedAt >= transactionFilter.Start.Value);
        }
        if (transactionFilter.End.HasValue)
        {
            var endOfDay = transactionFilter.End.Value.Date.AddDays(1).AddTicks(-1);
            AddCriteria(x => x.CreatedAt <= endOfDay);
        }
        if (transactionFilter.InvoiceId.HasValue)
        {
            AddCriteria(x => x.InvoiceId == new InvoiceId(transactionFilter.InvoiceId.Value));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(InventoryStock.CreatedAt), SortDirection.Ascending);
        }

        // --- Paging ---
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 15;
        ApplyPaging(skip, take);
    }
}