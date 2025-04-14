using System.Linq.Dynamic.Core;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.Domain.Aggregates.CustomerAggregate;
using GoldEx.Shared.Domain.Aggregates.TransactionAggregate;
using GoldEx.Shared.Domain.Entities;
using GoldEx.Shared.Infrastructure.Extensions;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Shared.Infrastructure.Repositories;

public class TransactionRepository<TTransaction, TCustomer>(IGoldExDbContextFactory factory)
    : RepositoryBase<TTransaction>(factory), ITransactionRepository<TTransaction, TCustomer>
    where TTransaction : TransactionBase<TCustomer>
    where TCustomer : CustomerBase
{
    public async Task<TTransaction?> GetAsync(TransactionId id, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TTransaction?> GetAsync(int number, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        return await NonDeletedQuery.FirstOrDefaultAsync(x => x.Number == number, cancellationToken);
    }

    public async Task<PagedList<TTransaction>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        if (filter.Skip < 0)
            filter.Skip = 0;

        var query = NonDeletedQuery
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(filter.Search))
        {
            if (int.TryParse(filter.Search, out var number))
            {
                query = query.Where(x =>
                    x.Number.Equals(number));
            }
            else
            {
                query = query.Where(x =>
                    x.Customer.FullName.Contains(filter.Search) ||
                    (!string.IsNullOrEmpty(x.Description) && x.Description.Contains(filter.Search)));
            }
        }

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

        return new PagedList<TTransaction> { Data = list, Skip = filter.Skip ?? 0, Take = filter.Take ?? 100, Total = totalRecords };
    }

    public async Task<List<TTransaction>> GetPendingItemsAsync(DateTime checkpointDate, CancellationToken cancellationToken = default)
    {
        await InitializeDbContextAsync();

        // ClientSide pending items
        if (typeof(TTransaction).IsAssignableTo(typeof(ITrackableEntity)))
        {
            return await AllQuery
                .Include(x => x.Customer)
                .Where($"{nameof(ITrackableEntity.Status)}<>{(int)ModifyStatus.Synced}")
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        // Serverside pending items
        if (typeof(TTransaction).IsAssignableTo(typeof(ISoftDeleteEntity)))
        {
            return await AllQuery
                .Include(x => x.Customer)
                .Where(x => x.LastModifiedDate >= checkpointDate)
                .OrderBy(x => x.LastModifiedDate)
                .ToListAsync(cancellationToken);
        }

        return [];
    }

    public async Task<int> GetLatestTransactionNumberAsync(CancellationToken cancellationToken = default)
    {
        var latestTransaction = await AllQuery
            .OrderByDescending(x => x.Number)
            .FirstOrDefaultAsync(cancellationToken);

        return latestTransaction?.Number ?? 0;
    }
}