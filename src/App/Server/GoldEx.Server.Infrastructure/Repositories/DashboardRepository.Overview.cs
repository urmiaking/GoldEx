using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Shared.DTOs.Dashboard;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

internal partial class DashboardRepository
{
    public async Task<DashboardOverviewResponse> GetOverviewAsync(DateOnly today, int days, CancellationToken cancellationToken = default)
    {
        if (days is not (7 or 30 or 90))
            throw new ArgumentOutOfRangeException(nameof(days));

        var start = today.AddDays(1 - days);
        var previousStart = start.AddDays(-days);
        var invoices = dbContext.Set<Invoice>().AsNoTracking();

        // Project component sums independently. SQL Server promotes sums to decimal(38)
        // and can lose fractional digits when several promoted totals are added together.
        var trades = await WithAmounts(invoices.Where(x => x.InvoiceDate >= previousStart && x.InvoiceDate <= today))
            .Select(x => new
            {
                x.Invoice.InvoiceDate, x.Invoice.InvoiceType, x.Invoice.PriceUnitId, x.Invoice.CustomerId,
                PriceUnit = x.Invoice.PriceUnit == null ? "واحد نامشخص" : x.Invoice.PriceUnit.Title,
                x.Amounts
            })
            .ToListAsync(cancellationToken);

        var current = trades.Where(x => x.InvoiceDate >= start).ToList();
        var units = trades.GroupBy(x => new { x.PriceUnitId, x.PriceUnit })
            .OrderByDescending(g => g.Count(x => x.InvoiceDate >= start))
            .ThenBy(g => g.Key.PriceUnit, StringComparer.Ordinal)
            .ThenBy(g => g.Key.PriceUnitId.Value)
            .Select(g =>
            {
                var points = g.Where(x => x.InvoiceDate >= start).GroupBy(x => x.InvoiceDate)
                    .ToDictionary(d => d.Key, d => new DashboardTradePoint(
                        d.Key, d.Where(x => x.InvoiceType == InvoiceType.Sell).Sum(x => x.Amounts.Total(x.InvoiceType)),
                        d.Where(x => x.InvoiceType == InvoiceType.Purchase).Sum(x => x.Amounts.Total(x.InvoiceType))));
                return new DashboardTradeSeries(g.Key.PriceUnitId.Value, g.Key.PriceUnit,
                    points.Values.Sum(x => x.SellAmount),
                    points.Values.Sum(x => x.PurchaseAmount),
                    g.Where(x => x.InvoiceDate < start && x.InvoiceType == InvoiceType.Sell).Sum(x => x.Amounts.Total(x.InvoiceType)),
                    g.Where(x => x.InvoiceDate < start && x.InvoiceType == InvoiceType.Purchase).Sum(x => x.Amounts.Total(x.InvoiceType)),
                    Enumerable.Range(0, days).Select(i =>
                    {
                        var date = start.AddDays(i);
                        return points.GetValueOrDefault(date) ?? new DashboardTradePoint(date, 0m, 0m);
                    }).ToList());
            })
            .ToList();

        // Stream the due-date candidates once. Keep only five rows for display and
        // calculate the full counts without loading invoice entities or their collections.
        var dueCandidates = WithAmounts(invoices.Where(x => x.DueDate <= today && x.InvoiceDate <= today))
            .OrderBy(x => x.Invoice.DueDate).ThenBy(x => x.Invoice.InvoiceDate)
            .ThenBy(x => x.Invoice.InvoiceNumber).ThenBy(x => x.Invoice.InvoiceType)
            .Select(x => new
            {
                x.Invoice.InvoiceNumber, x.Invoice.InvoiceType, DueDate = x.Invoice.DueDate!.Value,
                Customer = x.Invoice.Customer == null ? "مشتری بدون نام" : x.Invoice.Customer.FullName,
                PriceUnit = x.Invoice.PriceUnit == null ? "واحد نامشخص" : x.Invoice.PriceUnit.Title,
                x.Amounts,
                Received = x.Invoice.InvoicePayments!.Where(p => p.PaymentSide == PaymentSide.Receive)
                    .Sum(p => (decimal?)(p.FinalAmount * (p.ExchangeRate ?? 1m))) ?? 0m,
                Paid = x.Invoice.InvoicePayments!.Where(p => p.PaymentSide == PaymentSide.Pay)
                    .Sum(p => (decimal?)(p.FinalAmount * (p.ExchangeRate ?? 1m))) ?? 0m
            });

        var overdueCount = 0;
        var dueTodayCount = 0;
        var due = new List<DashboardDueInvoice>(5);
        await foreach (var invoice in dueCandidates.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            var total = invoice.Amounts.Total(invoice.InvoiceType);
            var remaining = invoice.InvoiceType == InvoiceType.Sell
                ? total - invoice.Amounts.Used - invoice.Received + invoice.Paid
                : total - invoice.Paid + invoice.Received;
            if (remaining <= 0m)
                continue;

            if (invoice.DueDate == today)
                dueTodayCount++;
            else
                overdueCount++;

            if (due.Count < 5)
                due.Add(new(invoice.InvoiceNumber, invoice.InvoiceType, invoice.DueDate, invoice.Customer, remaining, invoice.PriceUnit));
        }

        var paymentStart = start.ToDateTime(TimeOnly.MinValue);
        var paymentEnd = today.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var settlements = await dbContext.Set<InvoicePayment>().AsNoTracking()
            .Where(x => x.PaymentType == PaymentType.InternalCash && x.PaymentDate >= paymentStart && x.PaymentDate < paymentEnd)
            .GroupBy(x => new { x.PriceUnitId, Unit = x.PriceUnit == null ? "واحد نامشخص" : x.PriceUnit.Title })
            .OrderByDescending(g => g.Count()).ThenBy(g => g.Key.Unit).ThenBy(g => g.Key.PriceUnitId)
            .Select(g => new DashboardSettlementSummary(g.Key.Unit,
                g.Sum(x => x.PaymentSide == PaymentSide.Receive ? x.FinalAmount : 0m),
                g.Sum(x => x.PaymentSide == PaymentSide.Pay ? x.FinalAmount : 0m),
                g.Count()))
            .ToListAsync(cancellationToken);

        return new(start, today,
            current.Count(x => x.InvoiceType == InvoiceType.Sell),
            current.Count(x => x.InvoiceType == InvoiceType.Purchase),
            current.Select(x => x.CustomerId).Distinct().Count(),
            units, settlements, overdueCount, dueTodayCount, due);
    }

    private static IQueryable<InvoiceProjection> WithAmounts(IQueryable<Invoice> invoices) =>
        invoices.Select(x => new InvoiceProjection
        {
            Invoice = x,
            Amounts = new InvoiceAmounts(
                x.ProductItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
                x.ProductItems.Sum(i => (decimal?)i.ItemStoneAmount) ?? 0m,
                x.CoinItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
                x.CurrencyItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
                x.UsedProducts.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
                x.Discounts.Sum(i => (decimal?)(i.Amount * (i.ExchangeRate ?? 1m))) ?? 0m,
                x.ExtraCosts.Sum(i => (decimal?)(i.Amount * (i.ExchangeRate ?? 1m))) ?? 0m)
        });

    private sealed class InvoiceProjection
    {
        public required Invoice Invoice { get; init; }
        public required InvoiceAmounts Amounts { get; init; }
    }

    private sealed record InvoiceAmounts(decimal Product, decimal Stone, decimal Coin, decimal Currency,
        decimal Used, decimal Discount, decimal ExtraCost)
    {
        // Mirrors Invoice.TotalAmountWithDiscountsAndExtraCosts.
        public decimal Total(InvoiceType type) => Product + Stone + Coin + Currency
            + (type == InvoiceType.Purchase ? Used : 0m) - Discount + ExtraCost;
    }
}
