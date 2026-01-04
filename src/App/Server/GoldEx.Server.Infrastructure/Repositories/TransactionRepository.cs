using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class TransactionRepository(GoldExDbContext dbContext) : RepositoryBase<Transaction>(dbContext), ITransactionRepository
{
    public async Task RemoveByInvoiceIdAsync(InvoiceId invoiceId, CancellationToken cancellationToken = default)
    {
        var transactions = await Query
            .Where(t => t.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);

        if (transactions.Count == 0)
            return;

        await DeleteRangeAsync(transactions, cancellationToken);
    }

    public async Task RemoveByPaymentVoucherIdAsync(PaymentVoucherId paymentVoucherId, CancellationToken cancellationToken = default)
    {
        var transactions = await Query
            .Where(t => t.PaymentVoucherId == paymentVoucherId)
            .ToListAsync(cancellationToken);

        if (transactions.Count == 0)
            return;

        await DeleteRangeAsync(transactions, cancellationToken);
    }

    public async Task RemoveByInvoicePaymentIdsAsync(List<InvoicePaymentId>? invoicePaymentIds,
        CancellationToken cancellationToken = default)
    {
        if (invoicePaymentIds == null || invoicePaymentIds.Count == 0)
            return;

        var transactions = await Query
            .Where(t => t.InvoicePaymentId.HasValue && invoicePaymentIds.Contains(t.InvoicePaymentId.Value))
            .ToListAsync(cancellationToken);

        if (transactions.Count == 0)
            return;

        await DeleteRangeAsync(transactions, cancellationToken);
    }

    public async Task<Dictionary<PriceUnit, decimal>> GetCustomerRemainingListAsync(CustomerId customerId, PriceUnitId? priceUnitId, DateTime? untilDate = null,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = Query
            .Include(x => x.LedgerAccount!.PriceUnit)
            .Where(t => t.LedgerAccount != null && t.LedgerAccount.CustomerId == customerId);

        if (priceUnitId.HasValue)
        {
            baseQuery = baseQuery.Where(t => t.LedgerAccount!.PriceUnitId == priceUnitId.Value);
        }

        if (untilDate.HasValue)
        {
            baseQuery = baseQuery.Where(t => t.PostingDate < untilDate.Value);
        }

        var balances = await baseQuery
            .GroupBy(t => t.LedgerAccount!.PriceUnit)
            .Select(group => new
            {
                PriceUnit = group.Key,
                Balance = group.Sum(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount)
            })
            .AsNoTracking()
            .ToDictionaryAsync(result => result.PriceUnit!, result => result.Balance, cancellationToken);

        return balances;
    }

    public async Task<(decimal qty, decimal baseAmount, decimal avgRate)> GetLedgerPositionSummaryAsync(LedgerAccountId ledgerAccountId,
        CancellationToken cancellationToken = default)
    {
        var q = Query
            .Where(x => x.LedgerAccountId == ledgerAccountId && x.ReverseTransactionId == null)
            .OrderByDescending(x => x.PostingDate)
            .AsNoTracking();

        // Qty: بدهکار + ، بستانکار -
        var qtySum = await q
            .Select(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount)
            .SumAsync(cancellationToken);

        // Base: از BaseCurrencyAmount جمع با علامت مشابه
        var baseSum = await q
            .Select(t => (t.TransactionType == TransactionType.Debit ? t.BaseCurrencyAmount : -t.BaseCurrencyAmount))
            .SumAsync(cancellationToken);

        var avg = qtySum != 0 ? baseSum / qtySum : 0;
        return (qtySum, baseSum, avg);
    }

    public async Task<List<AccountBalanceSummaryModel>> GetPayableReceivableAccountsSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = Query
            .AsNoTracking()
            .Include(t => t.PriceUnit)
            .Include(x => x.ReversedBy)
            .Include(t => t.LedgerAccount!).ThenInclude(la => la.ParentAccount)
            .Where(t => !fromDate.HasValue || t.PostingDate >= fromDate.Value)
            .Where(t => !toDate.HasValue || t.PostingDate <= toDate.Value)
            .Where(t =>
                t.LedgerAccount != null &&
                t.LedgerAccount.ParentAccount != null &&
                (
                    t.LedgerAccount.ParentAccount.Title == SystemLedgerAccounts.AccountsPayable ||
                    t.LedgerAccount.ParentAccount.Title == SystemLedgerAccounts.AccountsReceivable
                ))
            .Where(t => t.ReverseTransactionId == null)
            .Where(x => x.ReversedBy == null || !x.ReversedBy.Any());

        var result = await query
            .GroupBy(t => new { t.PriceUnitId, t.PriceUnit!.Title })
            .Select(g => new AccountBalanceSummaryModel(
                g.Key.Title,
                g.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.Amount),
                g.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.Amount)))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<LedgerAccountTrialBalanceModel> GetLedgerAccountTrialBalanceAsync(
    LedgerAccountTrialBalanceRpRequest request,
    CancellationToken cancellationToken)
    {
        var basePriceUnit = await dbContext
            .Set<PriceUnit>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsDefault, cancellationToken)
                ?? throw new InvalidOperationException("Default price unit not found.");

        // 1. DISPLAY DATA: Fetch only System Accounts
        var visibleLedgers = await dbContext.Set<LedgerAccount>()
            .AsNoTracking()
            .Where(x => x.IsSystemAccount)
            .Select(x => new
            {
                x.Id,
                x.Title,
                x.AccountType,
                x.ParentAccountId
            })
            .ToListAsync(cancellationToken);

        if (visibleLedgers.Count == 0)
        {
            return new LedgerAccountTrialBalanceModel
            {
                BasePriceUnitTitle = basePriceUnit.Title,
                Nodes = []
            };
        }

        // 2. LOGIC DATA: Fetch Hierarchy for ALL accounts (including non-system)
        var allHierarchy = await dbContext.Set<LedgerAccount>()
            .AsNoTracking()
            .Select(x => new { x.Id, x.ParentAccountId })
            .ToListAsync(cancellationToken);

        var globalChildrenLookup = allHierarchy
            .Where(x => x.ParentAccountId != null)
            .GroupBy(x => x.ParentAccountId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

        var globalParentLookup = allHierarchy
            .ToDictionary(x => x.Id, x => x.ParentAccountId);

        // 3. DETERMINE SUBTREE
        var subtreeLedgerIds = new HashSet<LedgerAccountId>();

        if (request.ParentLedgerId == null)
        {
            foreach (var h in allHierarchy)
                subtreeLedgerIds.Add(h.Id);
        }
        else
        {
            var rootId = new LedgerAccountId(request.ParentLedgerId.Value);

            if (!globalParentLookup.ContainsKey(rootId))
            {
                return new LedgerAccountTrialBalanceModel { BasePriceUnitTitle = basePriceUnit.Title, Nodes = [] };
            }

            var stack = new Stack<LedgerAccountId>();
            stack.Push(rootId);

            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (!subtreeLedgerIds.Add(cur)) continue;

                if (globalChildrenLookup.TryGetValue(cur, out var children))
                {
                    foreach (var ch in children)
                        stack.Push(ch);
                }
            }
        }

        // 4. FETCH TRANSACTIONS
        var txAgg = await Query
            .AsNoTracking()
            .Where(t => t.ReverseTransactionId == null)
            .Where(t => request.FromDate == null || t.PostingDate >= request.FromDate.Value)
            .Where(t => request.ToDate == null || t.PostingDate <= request.ToDate.Value)
            .Where(t => subtreeLedgerIds.Contains(t.LedgerAccountId))
            .GroupBy(t => t.LedgerAccountId)
            .Select(g => new
            {
                LedgerAccountId = g.Key,
                DebitBase = g.Where(x => x.TransactionType == TransactionType.Debit).Sum(x => x.BaseCurrencyAmount),
                CreditBase = g.Where(x => x.TransactionType == TransactionType.Credit).Sum(x => x.BaseCurrencyAmount),
            })
            .ToListAsync(cancellationToken);

        var debit = new Dictionary<LedgerAccountId, decimal>();
        var credit = new Dictionary<LedgerAccountId, decimal>();

        foreach (var x in txAgg)
        {
            debit[x.LedgerAccountId] = x.DebitBase;
            credit[x.LedgerAccountId] = x.CreditBase;
        }

        // 5. ROLL-UP LOGIC
        foreach (var leafId in debit.Keys.ToList())
        {
            var d = debit.GetValueOrDefault(leafId, 0m);
            var c = credit.GetValueOrDefault(leafId, 0m);

            var cur = leafId;
            while (true)
            {
                if (!globalParentLookup.TryGetValue(cur, out var parentId) || parentId == null) break;

                var parent = parentId.Value;
                debit[parent] = debit.GetValueOrDefault(parent, 0m) + d;
                credit[parent] = credit.GetValueOrDefault(parent, 0m) + c;
                cur = parent;
            }
        }

        // 6. TREE CONSTRUCTION (With Zero-Filtering)
        var info = visibleLedgers.ToDictionary(x => x.Id, x => x);

        var childrenMap = new Dictionary<LedgerAccountId, List<LedgerAccountId>>();
        foreach (var a in info.Values)
        {
            if (a.ParentAccountId == null) continue;
            var parent = a.ParentAccountId.Value;
            if (!info.ContainsKey(parent)) continue;

            if (!childrenMap.TryGetValue(parent, out var list))
                childrenMap[parent] = list = [];

            list.Add(a.Id);
        }

        List<LedgerAccountId> roots;
        if (request.ParentLedgerId.HasValue)
        {
            var root = new LedgerAccountId(request.ParentLedgerId.Value);
            roots = info.ContainsKey(root) ? [root] : [];
        }
        else
        {
            roots = info.Values
                .Where(x => x.ParentAccountId == null || !info.ContainsKey(x.ParentAccountId.Value))
                .OrderBy(x => x.Title)
                .Select(x => x.Id)
                .ToList();
        }

        // MODIFIED: Return nullable to indicate "hide this node"
        LedgerAccountTrialBalanceNodeModel? Build(LedgerAccountId id)
        {
            var d = debit.GetValueOrDefault(id, 0m);
            var c = credit.GetValueOrDefault(id, 0m);

            // FILTER: If both Debit and Credit are 0, this account (and its children) 
            // has no impact on the report.
            if (d == 0 && c == 0) return null;

            var a = info[id];
            var childs = childrenMap.TryGetValue(id, out var ch) ? ch : [];

            return new LedgerAccountTrialBalanceNodeModel
            {
                Id = id.Value,
                ParentAccountId = a.ParentAccountId?.Value,
                LedgerAccountTitle = a.Title,
                LedgerAccountType = a.AccountType,
                BasePriceUnitTitle = basePriceUnit.Title,
                DebitAmountBase = d,
                CreditAmountBase = c,
                SubLedgerAccounts = childs
                    .OrderBy(cid => info[cid].Title)
                    .Select(Build)
                    .Where(n => n != null) // Filter out the null children
                    .Cast<LedgerAccountTrialBalanceNodeModel>() // Fix type
                    .ToList()
            };
        }

        return new LedgerAccountTrialBalanceModel
        {
            BasePriceUnitTitle = basePriceUnit.Title,
            Nodes = roots
                .Select(Build)
                .Where(n => n != null) // Filter out null roots
                .Cast<LedgerAccountTrialBalanceNodeModel>()
                .ToList()
        };
    }
}