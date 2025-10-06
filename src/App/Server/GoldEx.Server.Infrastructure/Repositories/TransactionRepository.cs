using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.PaymentVoucherAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
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

    public async Task<Dictionary<PriceUnit, decimal>> GetCustomerRemainingListAsync(CustomerId customerId, CancellationToken cancellationToken = default)
    {
        var balances = await Query
            .Include(x => x.LedgerAccount!.PriceUnit)
            .Where(t => t.LedgerAccount != null && t.LedgerAccount.CustomerId == customerId)
            .GroupBy(t => t.LedgerAccount!.PriceUnit)
            .Select(group => new
            {
                PriceUnit = group.Key,
                Balance = group.Sum(t =>
                    // اگر نوع تراکنش بدهکار است، مبلغ مثبت و اگر بستانکار است، مبلغ منفی در نظر گرفته می‌شود
                    t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount)
            })
            .AsNoTracking()
            .ToDictionaryAsync(result => result.PriceUnit!, result => result.Balance, cancellationToken);

        return balances;
    }
}