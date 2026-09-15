using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Dashboard;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class DashboardRepository(
    GoldExDbContext dbContext,
    ITransactionRepository transactionRepository) : IDashboardRepository
{
    public async Task<List<TodaySalesSummaryDto>> GetTodaySalesSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayInvoices = await dbContext.Set<Invoice>()
            .AsNoTracking()
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems)
            .Include(x => x.CoinItems)
            .Include(x => x.CurrencyItems)
            .Include(x => x.UsedProducts)
            .Include(x => x.Discounts)
            .Include(x => x.ExtraCosts)
            .Where(x => x.InvoiceDate == today && x.InvoiceType == InvoiceType.Sell)
            .ToListAsync(cancellationToken);

        var sales = todayInvoices
            .GroupBy(x => x.PriceUnit != null ? x.PriceUnit.Title : "تومان")
            .Select(g => new TodaySalesSummaryDto
            {
                PriceUnit = g.Key,
                Amount = g.Sum(x => x.TotalAmountWithDiscountsAndExtraCosts),
                Count = g.Count(),
                Subtitle = $"تعداد فاکتور امروز: {g.Count()} عدد"
            })
            .ToList();

        if (!sales.Any())
        {
            sales.Add(new TodaySalesSummaryDto
            {
                PriceUnit = "تومان",
                Amount = 0,
                Count = 0,
                Subtitle = "امروز فاکتوری ثبت نشده است"
            });
        }

        return sales;
    }

    public async Task<CustomerBalancesSummaryDto> GetCustomerBalancesSummaryAsync(CancellationToken cancellationToken = default)
    {
        var balanceRequest = new CustomerRemainingBalanceRpRequest(null, null, null, null, null);
        var balances = await transactionRepository.GetCustomerRemainingBalanceAsync(balanceRequest, cancellationToken);

        var receivables = balances
            .Where(x => x.PayableAmount > 0)
            .GroupBy(x => x.PriceUnitTitle ?? "تومان")
            .Select(g => new PriceUnitSummaryDto
            {
                PriceUnit = g.Key,
                Amount = g.Sum(x => x.PayableAmount),
                Count = g.Count(),
                Subtitle = "مانده بدهکاری مشتریان به ما"
            })
            .ToList();

        if (!receivables.Any())
        {
            receivables.Add(new PriceUnitSummaryDto
            {
                PriceUnit = "تومان",
                Amount = 0,
                Count = 0,
                Subtitle = "هیچ طلبی از مشتریان ثبت نشده"
            });
        }

        var payables = balances
            .Where(x => x.ReceivableAmount > 0)
            .GroupBy(x => x.PriceUnitTitle ?? "تومان")
            .Select(g => new PriceUnitSummaryDto
            {
                PriceUnit = g.Key,
                Amount = g.Sum(x => x.ReceivableAmount),
                Count = g.Count(),
                Subtitle = "مانده بستانکاری مشتریان نزد ما"
            })
            .ToList();

        if (!payables.Any())
        {
            payables.Add(new PriceUnitSummaryDto
            {
                PriceUnit = "تومان",
                Amount = 0,
                Count = 0,
                Subtitle = "هیچ بدهی به مشتریان ثبت نشده"
            });
        }

        return new CustomerBalancesSummaryDto
        {
            Receivables = receivables,
            Payables = payables
        };
    }

    public async Task<List<TradeTrendPointDto>> GetTradeTrend30DaysAsync(CancellationToken cancellationToken = default)
    {
        var daysList = Enumerable.Range(0, 30)
            .Select(i => DateOnly.FromDateTime(DateTime.Today.AddDays(-29 + i)))
            .ToList();
        var startDate = daysList.First();

        var invoices = await dbContext.Set<Invoice>()
            .AsNoTracking()
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems)
            .Include(x => x.UsedProducts)
            .Include(x => x.Discounts)
            .Include(x => x.ExtraCosts)
            .Where(x => x.InvoiceDate >= startDate && (x.InvoiceType == InvoiceType.Sell || x.InvoiceType == InvoiceType.Purchase))
            .ToListAsync(cancellationToken);

        var trendQuery = invoices
            .GroupBy(x => new { x.InvoiceDate, x.InvoiceType })
            .Select(g => new
            {
                g.Key.InvoiceDate,
                g.Key.InvoiceType,
                TotalWeight = g.Sum(x => x.CalculateTotalWeightEquivalent())
            })
            .ToList();

        var sellDict = trendQuery
            .Where(x => x.InvoiceType == InvoiceType.Sell)
            .ToDictionary(x => x.InvoiceDate, x => x.TotalWeight);

        var purchaseDict = trendQuery
            .Where(x => x.InvoiceType == InvoiceType.Purchase)
            .ToDictionary(x => x.InvoiceDate, x => x.TotalWeight);

        return daysList.Select(d => new TradeTrendPointDto
        {
            Date = d,
            SellWeight = sellDict.GetValueOrDefault(d, 0m),
            PurchaseWeight = purchaseDict.GetValueOrDefault(d, 0m)
        }).ToList();
    }

    public async Task<List<TopUnpaidInvoiceDto>> GetTopUnpaidInvoicesAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        var candidates = await dbContext.Set<Invoice>()
            .AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.PriceUnit)
            .Include(x => x.ProductItems)
            .Include(x => x.CoinItems)
            .Include(x => x.CurrencyItems)
            .Include(x => x.UsedProducts)
            .Include(x => x.Discounts)
            .Include(x => x.ExtraCosts)
            .Include(x => x.InvoicePayments)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        var list = candidates
            .Where(x => Math.Abs(x.TotalUnpaidAmount) >= 0.01m)
            .OrderByDescending(x => x.TotalUnpaidAmount)
            .Take(count)
            .Select(x => new TopUnpaidInvoiceDto
            {
                Id = x.Id.Value,
                InvoiceNumber = x.InvoiceNumber.ToString(),
                InvoiceType = x.InvoiceType,
                CustomerFullName = x.Customer != null ? x.Customer.FullName : string.Empty,
                TotalUnpaidAmount = x.TotalUnpaidAmount,
                PriceUnit = x.PriceUnit != null ? x.PriceUnit.Title : "تومان",
                InvoiceDate = x.InvoiceDate
            })
            .ToList();

        if (!list.Any() && candidates.Any())
        {
            list = candidates
                .Take(count)
                .Select(x => new TopUnpaidInvoiceDto
                {
                    Id = x.Id.Value,
                    InvoiceNumber = x.InvoiceNumber.ToString(),
                    InvoiceType = x.InvoiceType,
                    CustomerFullName = x.Customer != null ? x.Customer.FullName : string.Empty,
                    TotalUnpaidAmount = x.TotalUnpaidAmount,
                    PriceUnit = x.PriceUnit != null ? x.PriceUnit.Title : "تومان",
                    InvoiceDate = x.InvoiceDate
                })
                .ToList();
        }

        return list;
    }
}
