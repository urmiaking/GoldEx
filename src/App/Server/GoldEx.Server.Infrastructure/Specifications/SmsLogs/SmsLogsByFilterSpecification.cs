using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.SmsLogAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.SmsLogs;

public class SmsLogsByFilterSpecification : SpecificationBase<SmsLog>
{
    public SmsLogsByFilterSpecification(RequestFilter requestFilter)
    {
        if (requestFilter.Skip < 0)
            requestFilter.Skip = 0;

        var skip = requestFilter.Skip ?? 0;
        var take = requestFilter.Take ?? 100;

        if (!string.IsNullOrEmpty(requestFilter.Search))
        {
            AddCriteria(x => x.Message.Contains(requestFilter.Search) || x.Receiver.Contains(requestFilter.Search));
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(requestFilter.SortLabel) && requestFilter.SortDirection != null && requestFilter.SortDirection != SortDirection.None)
        {
            ApplySorting(requestFilter.SortLabel, requestFilter.SortDirection.Value);
        }
        else
        {
            ApplySorting(nameof(SmsLog.CreatedAt), SortDirection.Descending);
        }

        // Apply paging
        ApplyPaging(skip, take);
    }
}