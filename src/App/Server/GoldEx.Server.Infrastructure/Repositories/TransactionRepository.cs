using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
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
}