using System.Linq.Dynamic.Core;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Extensions;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Shared.Infrastructure.Repositories;

public class CustomerRepository<TCustomer>(IGoldExDbContextFactory contextFactory) 
    : RepositoryBase<TCustomer>(contextFactory), 
        ICustomerRepository<TCustomer> where TCustomer : CustomerBase
{
    public async Task<TCustomer?> GetAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TCustomer?> GetAsync(string nationalId, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.NationalId == nationalId, cancellationToken);
    }

    public async Task<TCustomer?> GetByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);
    }

    public async Task<PagedList<TCustomer>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        if (filter.Skip < 0)
            filter.Skip = 0;

        var query = NonDeletedQuery
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(x =>
                x.FullName.Contains(filter.Search) ||
                x.NationalId.Contains(filter.Search) ||
                (!string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber.Contains(filter.Search)));

        // Apply sorting
        if (!string.IsNullOrEmpty(filter.SortLabel) && filter.SortDirection != null && filter.SortDirection != SortDirection.None)
        {
            query = query.ApplySorting(filter.SortLabel, filter.SortDirection.Value);
        }
        else
        {
            // Default sorting if no sort criteria is provided
            query = query.OrderByDescending(x => x.LastModifiedDate);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        query = query.Skip(filter.Skip ?? 0).Take(filter.Take ?? 100);

        var list = await query.ToListAsync(cancellationToken);

        return new PagedList<TCustomer> { Data = list, Skip = filter.Skip ?? 0, Take = filter.Take ?? 100, Total = totalRecords };
    }

    public async Task<List<TCustomer>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        // ClientSide pending items
        if (typeof(TCustomer).IsAssignableTo(typeof(ITrackableEntity)))
        {
            return await AllQuery
                .Where($"{nameof(ITrackableEntity.Status)}<>{(int)ModifyStatus.Synced}")
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        // Serverside pending items
        if (typeof(TCustomer).IsAssignableTo(typeof(ISoftDeleteEntity)))
        {
            return await AllQuery
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        return [];
    }
}