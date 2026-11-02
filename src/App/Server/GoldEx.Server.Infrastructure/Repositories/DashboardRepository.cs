using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Dashboard;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.Constants;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class DashboardRepository(GoldExDbContext dbContext) : IDashboardRepository
{
    public async Task<List<TodaySalesSummaryDto>> GetTodaySalesSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var rows = await dbContext.Set<Invoice>().AsNoTracking()
            .Where(x => x.InvoiceDate == today && x.InvoiceType == InvoiceType.Sell)
            .Select(x => new
            {
                Unit = x.PriceUnit != null ? x.PriceUnit.Title : "تومان",
                Amount = (x.ProductItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m)
                    + (x.ProductItems.Sum(i => (decimal?)i.ItemStoneAmount) ?? 0m)
                    + (x.CoinItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m)
                    + (x.CurrencyItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m)
                    - (x.Discounts.Sum(i => (decimal?)(i.Amount * (i.ExchangeRate ?? 1m))) ?? 0m)
                    + (x.ExtraCosts.Sum(i => (decimal?)(i.Amount * (i.ExchangeRate ?? 1m))) ?? 0m)
            })
            .ToListAsync(cancellationToken);
        var result = rows.GroupBy(x => x.Unit).Select(g => new TodaySalesSummaryDto
        {
            PriceUnit = g.Key, Amount = g.Sum(x => x.Amount), Count = g.Count(),
            Subtitle = $"تعداد فاکتور امروز: {g.Count()} عدد"
        }).ToList();
        if (result.Count == 0)
            result.Add(new TodaySalesSummaryDto
            {
                PriceUnit = "تومان", Amount = 0m, Count = 0,
                Subtitle = "امروز فاکتوری ثبت نشده است"
            });
        return result;
    }

    public async Task<CustomerBalancesSummaryDto> GetCustomerBalancesSummaryAsync(CancellationToken cancellationToken = default)
    {
        // The dashboard needs only grouped totals. Do not query the customer
        // directory report (name, national ID and phone per customer) here.
        var byCustomerAndUnit = await dbContext.Set<Transaction>()
            .AsNoTracking()
            .Where(t => t.LedgerAccount != null &&
                t.LedgerAccount.CustomerId != null &&
                t.LedgerAccount.ParentAccount != null &&
                (t.LedgerAccount.ParentAccount.Title == SystemLedgerAccounts.AccountsReceivable ||
                 t.LedgerAccount.ParentAccount.Title == SystemLedgerAccounts.AccountsPayable) &&
                t.ReverseTransactionId == null &&
                !t.ReversedBy!.Any())
            .GroupBy(t => new
            {
                t.LedgerAccount!.CustomerId,
                t.PriceUnitId,
                Unit = t.PriceUnit != null ? t.PriceUnit.Title : "تومان"
            })
            .Select(g => new
            {
                g.Key.Unit,
                Amount = g.Sum(t => t.TransactionType == TransactionType.Debit ? t.Amount : -t.Amount)
            })
            .ToListAsync(cancellationToken);

        var receivables = byCustomerAndUnit.Where(x => x.Amount > 0)
            .GroupBy(x => x.Unit)
            .Select(g => new PriceUnitSummaryDto
            {
                PriceUnit = g.Key,
                Amount = g.Sum(x => x.Amount),
                Count = g.Count(),
                Subtitle = "مانده بدهکاری مشتریان به ما"
            }).ToList();
        var payables = byCustomerAndUnit.Where(x => x.Amount < 0)
            .GroupBy(x => x.Unit)
            .Select(g => new PriceUnitSummaryDto
            {
                PriceUnit = g.Key,
                Amount = g.Sum(x => Math.Abs(x.Amount)),
                Count = g.Count(),
                Subtitle = "مانده بستانکاری مشتریان نزد ما"
            }).ToList();
        if (receivables.Count == 0)
            receivables.Add(new PriceUnitSummaryDto
            {
                PriceUnit = "تومان", Amount = 0, Count = 0,
                Subtitle = "هیچ طلبی از مشتریان ثبت نشده"
            });
        if (payables.Count == 0)
            payables.Add(new PriceUnitSummaryDto
            {
                PriceUnit = "تومان", Amount = 0, Count = 0,
                Subtitle = "هیچ بدهی به مشتریان ثبت نشده"
            });
        return new CustomerBalancesSummaryDto
        {
            Receivables = receivables,
            Payables = payables
        };
    }

    public async Task<List<TradeTrendPointDto>> GetTradeTrend30DaysAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startDate = today.AddDays(-29);
        // SQL projects only aggregate columns. The per-item division stays decimal in C#
        // to preserve Invoice.CalculateTotalWeightEquivalent semantics.
        var invoices = await dbContext.Set<Invoice>().AsNoTracking()
            .Where(x => x.InvoiceDate >= startDate && x.InvoiceDate <= today &&
                (x.InvoiceType == InvoiceType.Sell || x.InvoiceType == InvoiceType.Purchase))
            .Select(x => new
            {
                x.Id, x.InvoiceDate, x.InvoiceType,
                UnitType = x.PriceUnit != null ? x.PriceUnit.UnitType : null,
                ProductAmount = x.ProductItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
                StoneAmount = x.ProductItems.Sum(i => (decimal?)i.ItemStoneAmount) ?? 0m,
                CoinAmount = x.CoinItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
                CurrencyAmount = x.CurrencyItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
                UsedAmount = x.UsedProducts.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
                UsedWeight = x.UsedProducts.Sum(i => (decimal?)i.Weight) ?? 0m,
                ProductWeight = x.ProductItems.Sum(i => (decimal?)i.TotalWeight) ?? 0m,
                Discount = x.Discounts.Sum(i => (decimal?)(i.Amount * (i.ExchangeRate ?? 1m))) ?? 0m,
                ExtraCost = x.ExtraCosts.Sum(i => (decimal?)(i.Amount * (i.ExchangeRate ?? 1m))) ?? 0m
            }).ToListAsync(cancellationToken);

        var rows = await dbContext.Set<Invoice>()
            .AsNoTracking()
            .Where(invoice => invoice.InvoiceDate >= startDate && invoice.InvoiceDate <= today &&
                (invoice.InvoiceType == InvoiceType.Sell || invoice.InvoiceType == InvoiceType.Purchase) &&
                (invoice.PriceUnit == null ||
                 (invoice.PriceUnit.UnitType != UnitType.Gold18K &&
                  invoice.PriceUnit.UnitType != UnitType.Mesghal)))
            .SelectMany(invoice => invoice.ProductItems.Select(item => new
            {
                InvoiceId = invoice.Id,
                item.ItemFinalAmount,
                item.GramPrice
            }))
            .ToListAsync(cancellationToken);

        var equivalents = rows.GroupBy(i => i.InvoiceId).ToDictionary(
            g => g.Key,
            g => (
                Weight: g.Where(i => i.GramPrice > 0m).Sum(i => i.ItemFinalAmount / i.GramPrice),
                Final: g.Where(i => i.GramPrice > 0m).Sum(i => i.ItemFinalAmount)
            ));
        var totals = invoices.GroupBy(x => new { x.InvoiceDate, x.InvoiceType })
            .Select(g => new
            {
                g.Key.InvoiceDate,
                g.Key.InvoiceType,
                Amount = g.Sum(x =>
                {
                    var invoiceTotal = x.ProductAmount + x.StoneAmount + x.CoinAmount +
                        x.CurrencyAmount + (x.InvoiceType == InvoiceType.Purchase ? x.UsedAmount : 0m)
                        - x.Discount + x.ExtraCost;
                    if (x.UnitType is UnitType.Gold18K or UnitType.Mesghal)
                        return invoiceTotal;
                    if (!equivalents.TryGetValue(x.Id, out var item) || item.Weight == 0m)
                        return x.ProductWeight + x.UsedWeight;
                    var effectiveRate = item.Final / item.Weight;
                    return effectiveRate == 0m ? item.Weight :
                        item.Weight - x.Discount / effectiveRate + x.ExtraCost / effectiveRate;
                })
            }).ToList();
        var sells = totals.Where(x => x.InvoiceType == InvoiceType.Sell)
            .ToDictionary(x => x.InvoiceDate, x => x.Amount);
        var purchases = totals.Where(x => x.InvoiceType == InvoiceType.Purchase)
            .ToDictionary(x => x.InvoiceDate, x => x.Amount);
        return Enumerable.Range(0, 30).Select(i =>
        {
            var date = startDate.AddDays(i);
            return new TradeTrendPointDto
            {
                Date = date,
                SellWeight = sells.GetValueOrDefault(date),
                PurchaseWeight = purchases.GetValueOrDefault(date)
            };
        }).ToList();
    }

    public async Task<List<TopUnpaidInvoiceDto>> GetTopUnpaidInvoicesAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        // Exact financial arithmetic projected as SQL sums, with sorting and Take
        // performed in the database rather than limiting to 100 recent invoices.
        var amounts = dbContext.Set<Invoice>().AsNoTracking().Select(x => new
        {
            x.Id, x.InvoiceNumber, x.InvoiceType, x.InvoiceDate, x.DueDate,
            CustomerName = x.Customer != null ? x.Customer.FullName : string.Empty,
            PriceUnit = x.PriceUnit != null ? x.PriceUnit.Title : "تومان",
            Final = (x.ProductItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m)
                + (x.ProductItems.Sum(i => (decimal?)i.ItemStoneAmount) ?? 0m)
                + (x.CoinItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m)
                + (x.CurrencyItems.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m)
                + (x.InvoiceType == InvoiceType.Purchase
                    ? (x.UsedProducts.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m) : 0m)
                - (x.Discounts.Sum(i => (decimal?)(i.Amount * (i.ExchangeRate ?? 1m))) ?? 0m)
                + (x.ExtraCosts.Sum(i => (decimal?)(i.Amount * (i.ExchangeRate ?? 1m))) ?? 0m),
            Used = x.UsedProducts.Sum(i => (decimal?)i.ItemFinalAmount) ?? 0m,
            Receive = x.InvoicePayments!.Where(i => i.PaymentSide == PaymentSide.Receive)
                .Sum(i => (decimal?)(i.FinalAmount * (i.ExchangeRate ?? 1m))) ?? 0m,
            Pay = x.InvoicePayments!.Where(i => i.PaymentSide == PaymentSide.Pay)
                .Sum(i => (decimal?)(i.FinalAmount * (i.ExchangeRate ?? 1m))) ?? 0m
        });
        var ranked = await amounts.Select(x => new
            {
                x.Id, x.InvoiceNumber, x.InvoiceType, x.InvoiceDate, x.DueDate,
                x.CustomerName, x.PriceUnit,
                Remaining = x.InvoiceType == InvoiceType.Sell
                    ? x.Final - x.Used - x.Receive + x.Pay
                    : x.Final - x.Pay + x.Receive
            })
            .Where(x => x.Remaining >= 0.01m)
            .OrderByDescending(x => x.Remaining)
            .Take(Math.Clamp(count, 1, 20))
            .ToListAsync(cancellationToken);
        return ranked.Select(x => new TopUnpaidInvoiceDto
        {
            Id = x.Id.Value, InvoiceNumber = x.InvoiceNumber.ToString(),
            InvoiceType = x.InvoiceType, InvoiceDate = x.InvoiceDate,
            DueDate = x.DueDate, CustomerFullName = x.CustomerName,
            PriceUnit = x.PriceUnit, TotalUnpaidAmount = x.Remaining
        }).ToList();
    }

}