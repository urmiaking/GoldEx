using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoresByFilterSpecification : SpecificationBase<Store>
{
    public StoresByFilterSpecification(RequestFilter requestFilter)
    {
        if (requestFilter.Skip < 0)
            requestFilter.Skip = 0;

        var skip = requestFilter.Skip ?? 0;
        var take = requestFilter.Take ?? 10;

        if (!string.IsNullOrEmpty(requestFilter.Search))
        {
            AddCriteria(x => x.Name.Contains(requestFilter.Search) || x.Slug.Contains(requestFilter.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(requestFilter.SortLabel) && requestFilter.SortDirection != null && requestFilter.SortDirection != SortDirection.None)
        {
            ApplySorting(requestFilter.SortLabel, requestFilter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(Store.Name), SortDirection.Ascending);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}
