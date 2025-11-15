using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryEntryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryEntries;

public class InventoryEntriesByFilterSpecification : SpecificationBase<InventoryEntry>
{
    public InventoryEntriesByFilterSpecification(RequestFilter requestFilter)
    {
        if (requestFilter.Skip < 0)
            requestFilter.Skip = 0;

        var skip = requestFilter.Skip ?? 0;
        var take = requestFilter.Take ?? 100;

        // Apply sorting
        if (!string.IsNullOrEmpty(requestFilter.SortLabel) && requestFilter.SortDirection != null && requestFilter.SortDirection != SortDirection.None)
        {
            ApplySorting(requestFilter.SortLabel, requestFilter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(InventoryEntry.CreatedAt), SortDirection.Descending);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}