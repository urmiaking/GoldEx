using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByFilterSpecification : SpecificationBase<Invoice>
{
    public InvoicesByFilterSpecification(RequestFilter filter, CustomerId? customerId)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        AddInclude(x => x.Customer!);
        AddInclude(x => x.PriceUnit!);

        // Apply customer filter if provided
        if (customerId.HasValue)
        {
            AddCriteria(x => x.CustomerId == customerId.Value);
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(filter.Search))
        {
            if (int.TryParse(filter.Search, out var number))
            {
                AddCriteria(x => x.InvoiceNumber.Equals(number));
            }
            else
            {
                AddCriteria(x =>
                    x.Customer!.FullName.Contains(filter.Search));
            }
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            // Default sorting if no sort criteria is provided
            ApplySorting(nameof(Invoice.CreatedAt), SortDirection.Descending);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}