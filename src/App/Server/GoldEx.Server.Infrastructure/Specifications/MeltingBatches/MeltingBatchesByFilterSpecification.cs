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
            if (int.TryParse(requestFilter.Search, out var batchNumber))
            {
                AddCriteria(x => x.BatchNumber == batchNumber);
            }
            else if (Guid.TryParse(requestFilter.Search, out var id))
            {
                AddCriteria(x => x.Id == new MeltingBatchId(id));
            }
        }

        if (filter.Status.HasValue)
        {
            AddCriteria(x => x.ChangeLogs.Max(y => y.Status) == filter.Status);
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
            AddCriteria(x => x.ChangeLogs.Max(y => y.DateTime) >= filter.Start.Value);
        }
        if (filter.End.HasValue)
        {
            var endOfDay = filter.End.Value.Date.AddDays(1).AddTicks(-1);
            AddCriteria(x => x.ChangeLogs.Max(y => y.DateTime) <= endOfDay);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}