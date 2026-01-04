using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

//[ScopedService]
internal sealed class TransactionFixSeeder(
    ITransactionRepository repository,
    IPriceUnitRepository priceUnitRepository,
    ILogger<TransactionFixSeeder> logger) : IDbSeeder
{
    public int Order => 999;
    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        var basePriceUnitId = await priceUnitRepository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .Select(x => x.Id)
            .SingleAsync(cancellationToken);

        var brokenTransactions = await repository
            .Get(new TransactionDefaultSpecification(false))
            .Include(t => t.Invoice)
            .Include(t => t.InvoicePayment)
            .Where(t =>
                t.InvoicePaymentId != null &&
                t.PriceUnitId != basePriceUnitId &&
                (
                    // پرداخت هم‌ارز فاکتور بوده ولی نرخ فاکتور اعمال نشده
                    (
                        t.InvoicePayment!.PriceUnitId == t.Invoice!.PriceUnitId &&
                        t.Invoice.ExchangeRate != null &&
                        t.ExchangeRate == null
                    )
                    || 
                    // پرداخت واحد متفاوت داشته و انتظار زنجیره‌ای بوده
                    (
                        t.InvoicePayment!.PriceUnitId != t.Invoice!.PriceUnitId &&
                        t.Invoice.ExchangeRate != null
                    )
                )
            )
            .ToListAsync(cancellationToken);

        if (brokenTransactions.Count == 0)
        {
            logger.LogInformation("No broken transactions found.");
            return;
        }

        logger.LogInformation("Found {Count} broken transactions. Fixing...", brokenTransactions.Count);

        foreach (var tx in brokenTransactions)
        {
            var invoice = tx.Invoice!;
            var payment = tx.InvoicePayment!;

            var oldRate = tx.ExchangeRate;

            var newRate = ResolveTransactionExchangeRate(
                invoice, payment, basePriceUnitId);

            tx.SetExchangeRate(newRate);

            logger.LogInformation(
                "Fixed Transaction {TransactionId}: ExchangeRate {OldRate} -> {NewRate}",
                tx.Id, oldRate?.ToString() ?? "null", newRate?.ToString() ?? "null");
        }

        await repository.UpdateRangeAsync(brokenTransactions, cancellationToken);
    }

    private static decimal? ResolveTransactionExchangeRate(
        Invoice invoice,
        InvoicePayment payment,
        PriceUnitId basePriceUnitId)
    {
        if (payment.PriceUnitId == basePriceUnitId)
            return null;

        if (payment.ExchangeRate.HasValue && invoice.PriceUnitId == basePriceUnitId)
            return payment.ExchangeRate;

        if (payment.PriceUnitId == invoice.PriceUnitId)
            return invoice.ExchangeRate;

        if (payment.ExchangeRate.HasValue && invoice.ExchangeRate.HasValue)
            return payment.ExchangeRate.Value * invoice.ExchangeRate.Value;

        return null;
    }
}