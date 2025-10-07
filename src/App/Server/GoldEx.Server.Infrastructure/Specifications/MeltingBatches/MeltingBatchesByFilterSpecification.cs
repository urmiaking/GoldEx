using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Shared.DTOs.MeltingBatches;

namespace GoldEx.Server.Infrastructure.Specifications.MeltingBatches;

public class MeltingBatchesByFilterSpecification : SpecificationBase<MeltingBatch>
{
    public MeltingBatchesByFilterSpecification(RequestFilter requestFilter, MeltingBatchFilter filter)
    {
        if (requestFilter.Skip < 0)
            requestFilter.Skip = 0;

        var skip = requestFilter.Skip ?? 0;
        var take = requestFilter.Take ?? 100;

        // Apply search filter
        if (!string.IsNullOrEmpty(requestFilter.Search))
        {
            AddCriteria(x => x.Description.Contains(requestFilter.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(requestFilter.SortLabel) && requestFilter.SortDirection != null && requestFilter.SortDirection != SortDirection.None)
        {
            ApplySorting(requestFilter.SortLabel, requestFilter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(MeltingBatch.CreatedAt), SortDirection.Descending);
        }

        // Apply date range filter on melting batch
        if (filter.Start.HasValue)
        {
            AddCriteria(x => x.CreatedAt >= filter.Start.Value);
        }
        if (filter.End.HasValue)
        {
            AddCriteria(x => x.CreatedAt <= filter.End.Value);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}