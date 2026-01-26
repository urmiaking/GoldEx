using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryExitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InventoryExits;

public class InventoryExitsByFilterSpecification : SpecificationBase<InventoryExit>
{
    public InventoryExitsByFilterSpecification(RequestFilter requestFilter)
    {
        if (requestFilter.Skip < 0)
            requestFilter.Skip = 0;

        var skip = requestFilter.Skip ?? 0;
        var take = requestFilter.Take ?? 100;

        AddInclude(x => x.InventoryStocks!);

        if (!string.IsNullOrEmpty(requestFilter.Search))
        {
            if (Guid.TryParse(requestFilter.Search, out var id))
            {
                AddCriteria(x => x.Id == new InventoryExitId(id));
            }
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(requestFilter.SortLabel) && requestFilter.SortDirection != null && requestFilter.SortDirection != SortDirection.None)
        {
            ApplySorting(requestFilter.SortLabel, requestFilter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(InventoryExit.CreatedAt), SortDirection.Descending);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}