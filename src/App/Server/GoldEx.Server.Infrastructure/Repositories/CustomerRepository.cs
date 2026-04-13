using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class CustomerRepository(GoldExDbContext dbContext)
    : RepositoryBase<Customer>(dbContext), ICustomerRepository
{
    public async Task<PagedList<Customer>> GetListAsync(
        CustomerFilter customerFilter,
        RequestFilter filter,
        CancellationToken cancellationToken = default)
    {
        var skip = filter.Skip ?? 0;
        var take = filter.Take ?? 100;

        if (skip < 0) skip = 0;
        if (take <= 0) take = 100;

        var query = Query
            .SetTracking(false)
            .Include(c => c.FinancialAccounts!)
            .ThenInclude(fa => fa.PriceUnit)
            .AsQueryable();

        #region Search

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(c =>
                c.FullName.Contains(search) ||
                c.NationalId.Contains(search) ||
                (!string.IsNullOrEmpty(c.PhoneNumber) && c.PhoneNumber.Contains(search)));
        }

        #endregion

        #region CustomerType

        if (customerFilter.CustomerType.HasValue)
        {
            query = query.Where(c => c.CustomerType == customerFilter.CustomerType.Value);
        }

        #endregion

        #region Date Range

        if (customerFilter.Start.HasValue)
            query = query.Where(c => c.CreatedAt >= customerFilter.Start.Value);

        if (customerFilter.End.HasValue)
        {
            var endOfDay = customerFilter.End.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(c => c.CreatedAt <= endOfDay);
        }

        #endregion

        #region Debit / Credit Filter ✅

        if (customerFilter.TransactionType.HasValue)
        {
            var balances =
                from t in dbContext.Set<Transaction>()
                    .SetTracking(false)
                where t.ReverseTransactionId == null
                   && !t.ReversedBy!.Any()
                   && t.LedgerAccount != null
                   && t.LedgerAccount.ParentAccount != null
                   && (
                        t.LedgerAccount.ParentAccount.Title == SystemLedgerAccounts.AccountsReceivable ||
                        t.LedgerAccount.ParentAccount.Title == SystemLedgerAccounts.AccountsPayable
                      )
                group t by t.LedgerAccount!.CustomerId into g
                select new
                {
                    CustomerId = g.Key,
                    Balance = g.Sum(x =>
                        x.TransactionType == TransactionType.Debit
                            ? x.Amount
                            : -x.Amount)
                };

            if (customerFilter.TransactionType == TransactionType.Debit)
            {
                query =
                    from c in query
                    join b in balances on c.Id equals b.CustomerId
                    where b.Balance > 0
                    select c;
            }
            else // Credit
            {
                query =
                    from c in query
                    join b in balances on c.Id equals b.CustomerId
                    where b.Balance < 0
                    select c;
            }
        }

        #endregion

        #region Sorting 

        query = query.ApplySorting(
            filter.SortLabel,
            filter.SortDirection ?? SortDirection.None,
            defaultSortProperty: "CreatedAt");

        #endregion

        var total = await query.CountAsync(cancellationToken);

        var data = await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return new PagedList<Customer>
        {
            Data = data,
            Skip = skip,
            Take = take,
            Total = total
        };
    }
}