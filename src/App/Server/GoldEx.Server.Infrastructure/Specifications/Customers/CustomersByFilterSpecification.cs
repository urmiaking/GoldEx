using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.DTOs.Customers;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByFilterSpecification : SpecificationBase<Customer>
{
    public CustomersByFilterSpecification(RequestFilter filter, CustomerFilter customerFilter)
    {
        if (filter.Skip < 0)
            filter.Skip = 0;

        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        AddInclude(x => x.CreditLimitPriceUnit!);

        // Apply search filter
        if (!string.IsNullOrEmpty(filter.Search))
        {
            AddCriteria(x =>
                x.FullName.Contains(filter.Search) ||
                x.NationalId.Contains(filter.Search) ||
                (!string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber.Contains(filter.Search)));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(Customer.CreatedAt), SortDirection.Descending);
        }

        // Apply date range filter on customer
        if (customerFilter.Start.HasValue)
        {
            AddCriteria(x => x.CreatedAt >= customerFilter.Start.Value);
        }
        if (customerFilter.End.HasValue)
        {
            AddCriteria(x => x.CreatedAt <= customerFilter.End.Value);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}