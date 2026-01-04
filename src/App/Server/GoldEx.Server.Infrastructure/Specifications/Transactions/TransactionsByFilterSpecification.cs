using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.DTOs.Transactions;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByFilterSpecification : SpecificationBase<Transaction>
{
    public TransactionsByFilterSpecification(TransactionFilter transactionFilter, RequestFilter? filter)
    {
        // Includes
        AddInclude(x => x.PriceUnit!);
        AddInclude(x => x.LedgerAccount!);

        // Apply filter
        if (transactionFilter.InvoiceId.HasValue)
        {
            AddCriteria(x => x.InvoiceId == new InvoiceId(transactionFilter.InvoiceId.Value));
        }
        if (transactionFilter.CustomerId.HasValue)
        {
            AddCriteria(x => x.LedgerAccount!.CustomerId == new CustomerId(transactionFilter.CustomerId.Value));
        }
        if (!transactionFilter.ShowReversed)
        {
            AddCriteria(t =>
                t.ReverseTransactionId == null &&
                t.ReversedBy!.Count == 0
            );
        }
        if (transactionFilter.Descending)
        {
            ApplyOrderByDescending(x => x.PostingDate);
        }
        if (transactionFilter.Start.HasValue)
        {
            AddCriteria(x => x.PostingDate >= transactionFilter.Start.Value);
        }
        if (transactionFilter.End.HasValue)
        {
            var endOfDay = transactionFilter.End.Value.Date.AddDays(1).AddTicks(-1);
            AddCriteria(x => x.PostingDate <= endOfDay);
        }
        if (transactionFilter.PriceUnitId.HasValue)
        {
            AddCriteria(x => x.PriceUnitId == new PriceUnitId(transactionFilter.PriceUnitId.Value));
        }

        if (transactionFilter.LedgerAccountId.HasValue)
        {
            AddCriteria(x => x.LedgerAccountId == new LedgerAccountId(transactionFilter.LedgerAccountId.Value));
        }

        if (filter != null)
        {
            if (!string.IsNullOrEmpty(filter.Search))
            {
                AddCriteria(x => x.Description.Contains(filter.Search));
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
            {
                ApplySorting(filter.SortLabel, filter.SortDirection.Value);
            }
            else
            {
                ApplySorting(nameof(Transaction.PostingDate), SortDirection.Ascending);
            }

            // --- Paging ---
            var skip = filter.Skip ?? 0;
            var take = filter.Take ?? 15;
            ApplyPaging(skip, take);
        }
    }
}