using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class InvoiceRepository(GoldExDbContext dbContext) : RepositoryBase<Invoice>(dbContext), IInvoiceRepository
{
    public async Task<long> GetLastNumberAsync(InvoiceType invoiceType, CancellationToken cancellationToken = default)
    {
        var invoiceNumber = await Query
            .Where(x => x.InvoiceType == invoiceType)
            .OrderByDescending(x => x.InvoiceNumber)
            .Select(x => x.InvoiceNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return invoiceNumber;
    }

    public async Task<List<Invoice>> GetOverdueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        return await Query
            .Include(x => x.PriceUnit)
            .Include(x => x.Customer)
            .Where(x => x.DueDate < DateOnly.FromDateTime(DateTime.Now) &&
                        !x.Notifications!.Any())
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<CategorySalesRpResponse>> GetCategorySalesSummaryAsync(
        CategorySalesRpRequest request, CancellationToken cancellationToken = default)
    {
        var baseQuery = Query
            .AsNoTracking()
            .Where(x => x.InvoiceType == InvoiceType.Sell);

        if (request.FromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(request.FromDate.Value.Date);
            baseQuery = baseQuery.Where(x => x.InvoiceDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = DateOnly.FromDateTime(request.ToDate.Value.Date);
            baseQuery = baseQuery.Where(x => x.InvoiceDate <= to);
        }

        if (request.PriceUnitId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.PriceUnitId == new PriceUnitId(request.PriceUnitId.Value));
        }

        // Fetch raw product item rows from invoices (strictly columns on InvoiceProductItems table)
        var rawItems = await baseQuery
            .SelectMany(inv => inv.ProductItems.Select(item => new
            {
                ProductId = item.ProductId.Value,
                item.TotalWeight,
                item.Quantity,
                WageAmount = item.ItemWageAmount,
                ProfitAmount = item.ItemProfitAmount,
                TaxAmount = item.ItemTaxAmount,
                FinalAmount = item.ItemFinalAmount
            }))
            .ToListAsync(cancellationToken);

        if (rawItems.Count == 0)
            return [];

        // Fetch category info for distinct products
        var productIds = rawItems.Select(x => new ProductId(x.ProductId)).Distinct().ToList();
        var products = await dbContext.Set<Product>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.ProductType,
                p.ProductCategoryId,
                CategoryTitle = p.ProductType == ProductType.MoltenGold
                    ? "طلای آبشده"
                    : (p.ProductCategory != null ? p.ProductCategory.Title : "متفرقه / سایر")
            })
            .ToListAsync(cancellationToken);

        var productsDict = products.ToDictionary(
            p => p.Id.Value,
            p => new
            {
                CategoryId = p.ProductType == ProductType.MoltenGold ? (Guid?)null : p.ProductCategoryId?.Value,
                p.CategoryTitle
            });

        var joinedItems = rawItems.Select(item =>
        {
            productsDict.TryGetValue(item.ProductId, out var prod);
            return new
            {
                CategoryId = prod?.CategoryId,
                CategoryTitle = prod?.CategoryTitle ?? "متفرقه / سایر",
                item.TotalWeight,
                item.Quantity,
                item.WageAmount,
                item.ProfitAmount,
                item.TaxAmount,
                item.FinalAmount
            };
        }).AsEnumerable();

        if (request.CategoryId.HasValue)
        {
            joinedItems = joinedItems.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryTitle))
        {
            var title = request.CategoryTitle.Trim();
            joinedItems = joinedItems.Where(x => x.CategoryTitle.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        var rawAggregates = joinedItems
            .GroupBy(x => new { x.CategoryId, x.CategoryTitle })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryTitle,
                TotalWeight = g.Sum(x => x.TotalWeight),
                TotalQuantity = g.Sum(x => x.Quantity),
                TotalAmount = g.Sum(x => x.FinalAmount),
                TotalProfit = g.Sum(x => x.ProfitAmount),
                TotalWage = g.Sum(x => x.WageAmount),
                TotalTax = g.Sum(x => x.TaxAmount),
                ItemCount = g.Count()
            })
            .OrderByDescending(x => x.TotalWeight)
            .ToList();

        var totalSumWeight = rawAggregates.Sum(x => x.TotalWeight);
        var totalSumAmount = rawAggregates.Sum(x => x.TotalAmount);

        return rawAggregates.Select(x => new CategorySalesRpResponse(
            x.CategoryId,
            x.CategoryTitle,
            x.TotalWeight,
            x.TotalQuantity,
            x.TotalAmount,
            x.TotalProfit,
            x.TotalWage,
            x.TotalTax,
            x.ItemCount,
            totalSumWeight > 0 ? Math.Round((x.TotalWeight / totalSumWeight) * 100m, 2) : 0m,
            totalSumAmount > 0 ? Math.Round((x.TotalAmount / totalSumAmount) * 100m, 2) : 0m
        )).ToList();
    }

    public async Task<List<SoldProductItemRpResponse>> GetSoldProductItemsAsync(
        SoldProductItemRpRequest request, CancellationToken cancellationToken = default)
    {
        var baseQuery = Query
            .AsNoTracking()
            .Where(x => x.InvoiceType == InvoiceType.Sell);

        if (request.FromDate.HasValue)
        {
            var from = DateOnly.FromDateTime(request.FromDate.Value.Date);
            baseQuery = baseQuery.Where(x => x.InvoiceDate >= from);
        }

        if (request.ToDate.HasValue)
        {
            var to = DateOnly.FromDateTime(request.ToDate.Value.Date);
            baseQuery = baseQuery.Where(x => x.InvoiceDate <= to);
        }

        var rawItems = await baseQuery.SelectMany(inv => inv.ProductItems.Select(item => new
        {
            InvoiceId = inv.Id.Value,
            InvoiceNumber = inv.InvoiceNumber,
            InvoiceDate = inv.InvoiceDate,
            CustomerName = inv.Customer != null ? inv.Customer.FullName : null,
            PriceUnit = inv.PriceUnit != null ? inv.PriceUnit.Title : "تومان",
            ProductId = item.ProductId.Value,
            item.TotalWeight,
            item.Quantity,
            item.GramPrice,
            WageAmount = item.ItemWageAmount,
            ProfitAmount = item.ItemProfitAmount,
            TaxAmount = item.ItemTaxAmount,
            FinalAmount = item.ItemFinalAmount
        })).ToListAsync(cancellationToken);

        if (rawItems.Count == 0)
            return [];

        var productIds = rawItems.Select(x => new ProductId(x.ProductId)).Distinct().ToList();
        var products = await dbContext.Set<Product>()
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                ProductName = p.Name,
                Barcode = p.Barcode,
                p.ProductType,
                p.ProductCategoryId,
                CategoryTitle = p.ProductType == ProductType.MoltenGold
                    ? "طلای آبشده"
                    : (p.ProductCategory != null ? p.ProductCategory.Title : "متفرقه / سایر")
            })
            .ToListAsync(cancellationToken);

        var productsDict = products.ToDictionary(
            p => p.Id.Value,
            p => new
            {
                p.ProductName,
                p.Barcode,
                CategoryId = p.ProductType == ProductType.MoltenGold ? (Guid?)null : p.ProductCategoryId?.Value,
                p.CategoryTitle
            });

        var joined = rawItems.Select(item =>
        {
            productsDict.TryGetValue(item.ProductId, out var prod);
            return new SoldProductItemRpResponse(
                item.InvoiceId,
                item.InvoiceNumber,
                item.InvoiceDate,
                item.CustomerName,
                item.ProductId,
                prod?.ProductName ?? "کالای طلا",
                prod?.Barcode,
                prod?.CategoryId,
                prod?.CategoryTitle ?? "سایر / بدون دسته‌بندی",
                item.TotalWeight,
                item.Quantity,
                item.GramPrice,
                item.WageAmount,
                item.ProfitAmount,
                item.TaxAmount,
                item.FinalAmount,
                item.PriceUnit
            );
        }).AsEnumerable();

        if (request.CategoryId.HasValue)
        {
            joined = joined.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryTitle))
        {
            var cat = request.CategoryTitle.Trim();
            joined = joined.Where(x => x.CategoryTitle.Contains(cat, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var q = request.SearchQuery.Trim();
            joined = joined.Where(x =>
                x.ProductName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                (x.Barcode != null && x.Barcode.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                (x.CustomerName != null && x.CustomerName.Contains(q, StringComparison.OrdinalIgnoreCase)) ||
                x.InvoiceNumber.ToString().Contains(q));
        }

        return joined
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.InvoiceNumber)
            .Skip(request.Skip)
            .Take(Math.Clamp(request.Take, 1, 500))
            .ToList();
    }

    public async Task<List<CategorySalesComparisonRpResponse>> GetCategorySalesComparisonAsync(
        CategorySalesComparisonRpRequest request, CancellationToken cancellationToken = default)
    {
        var list1 = await GetCategorySalesSummaryAsync(new CategorySalesRpRequest(
            request.FromDate1, request.ToDate1, request.CategoryId, request.CategoryTitle), cancellationToken);

        var list2 = await GetCategorySalesSummaryAsync(new CategorySalesRpRequest(
            request.FromDate2, request.ToDate2, request.CategoryId, request.CategoryTitle), cancellationToken);

        var allCategories = list1.Select(x => (x.CategoryId, x.CategoryTitle))
            .Union(list2.Select(x => (x.CategoryId, x.CategoryTitle)))
            .Distinct()
            .ToList();

        var result = new List<CategorySalesComparisonRpResponse>();

        foreach (var cat in allCategories)
        {
            var item1 = list1.FirstOrDefault(x => x.CategoryId == cat.CategoryId && x.CategoryTitle == cat.CategoryTitle);
            var item2 = list2.FirstOrDefault(x => x.CategoryId == cat.CategoryId && x.CategoryTitle == cat.CategoryTitle);

            var w1 = item1?.TotalWeight ?? 0m;
            var w2 = item2?.TotalWeight ?? 0m;
            var wDelta = w2 > 0 ? ((w1 - w2) / w2) * 100m : (w1 > 0 ? 100m : 0m);

            var q1 = item1?.TotalQuantity ?? 0;
            var q2 = item2?.TotalQuantity ?? 0;
            var qDelta = q2 > 0 ? (((decimal)q1 - q2) / q2) * 100m : (q1 > 0 ? 100m : 0m);

            var a1 = item1?.TotalAmount ?? 0m;
            var a2 = item2?.TotalAmount ?? 0m;
            var aDelta = a2 > 0 ? ((a1 - a2) / a2) * 100m : (a1 > 0 ? 100m : 0m);

            var p1 = item1?.TotalProfit ?? 0m;
            var p2 = item2?.TotalProfit ?? 0m;
            var pDelta = p2 > 0 ? ((p1 - p2) / p2) * 100m : (p1 > 0 ? 100m : 0m);

            result.Add(new CategorySalesComparisonRpResponse(
                cat.CategoryId,
                cat.CategoryTitle,
                w1,
                w2,
                Math.Round(wDelta, 2),
                q1,
                q2,
                Math.Round(qDelta, 2),
                a1,
                a2,
                Math.Round(aDelta, 2),
                p1,
                p2,
                Math.Round(pDelta, 2)
            ));
        }

        return result.OrderByDescending(x => x.Weight1).ThenByDescending(x => x.Weight2).ToList();
    }

    public async Task<InvoiceOverviewStatsResponse> GetOverviewStatsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var rawInvoices = await Query
            .AsNoTracking()
            .Select(x => new
            {
                x.Id,
                x.InvoiceType,
                x.InvoiceDate,
                x.DueDate,
                PriceUnitTitle = x.PriceUnit != null ? x.PriceUnit.Title : "تومان",
                ProductAmount = x.ProductItems.Sum(i => i.ItemFinalAmount + i.ItemStoneAmount),
                CoinAmount = x.CoinItems.Sum(c => c.ItemFinalAmount),
                CurrencyAmount = x.CurrencyItems.Sum(c => c.ItemFinalAmount),
                UsedProductAmount = x.UsedProducts.Sum(u => u.ItemFinalAmount),
                DiscountAmount = x.Discounts.Sum(d => d.Amount * (d.ExchangeRate ?? 1)),
                ExtraCostAmount = x.ExtraCosts.Sum(e => e.Amount * (e.ExchangeRate ?? 1)),
                ReceivedAmount = x.InvoicePayments!
                    .Where(p => p.PaymentSide == PaymentSide.Receive)
                    .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1)),
                PaidAmount = x.InvoicePayments!
                    .Where(p => p.PaymentSide == PaymentSide.Pay)
                    .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1))
            })
            .ToListAsync(cancellationToken);

        var calculated = rawInvoices.Select(item =>
        {
            var totalAmount = item.ProductAmount + item.CoinAmount + item.CurrencyAmount +
                              (item.InvoiceType == InvoiceType.Purchase ? item.UsedProductAmount : 0m);
            var totalWithDiscExtra = totalAmount - item.DiscountAmount + item.ExtraCostAmount;
            var netPaid = item.ReceivedAmount - item.PaidAmount;
            var totalPaid = item.PaidAmount - item.ReceivedAmount;

            var totalUnpaid = item.InvoiceType == InvoiceType.Sell
                ? (totalWithDiscExtra - item.UsedProductAmount - netPaid)
                : (totalWithDiscExtra - totalPaid);

            var isPaid = Math.Abs(totalUnpaid) < 0.01m;
            var isOverdue = !isPaid && item.DueDate.HasValue && item.DueDate.Value < today;
            var hasDebt = !isPaid && (!item.DueDate.HasValue || item.DueDate.Value >= today);

            return new
            {
                item.Id,
                item.InvoiceType,
                item.InvoiceDate,
                item.DueDate,
                item.PriceUnitTitle,
                TotalAmount = totalAmount,
                TotalUnpaid = totalUnpaid,
                IsPaid = isPaid,
                IsOverdue = isOverdue,
                HasDebt = hasDebt
            };
        }).ToList();

        var totalCount = calculated.Count;
        var sellCount = calculated.Count(x => x.InvoiceType == InvoiceType.Sell);
        var purchaseCount = calculated.Count(x => x.InvoiceType == InvoiceType.Purchase);
        var paidCount = calculated.Count(x => x.IsPaid);
        var debtCount = calculated.Count(x => x.HasDebt);
        var overdueCount = calculated.Count(x => x.IsOverdue);
        var averageValue = totalCount > 0 ? calculated.Average(x => x.TotalAmount) : 0m;

        // 1. Outstanding Unpaid Invoices by PriceUnit
        var unpaidGroups = calculated
            .Where(x => !x.IsPaid && x.TotalUnpaid > 0)
            .GroupBy(x => x.PriceUnitTitle)
            .Select(g => new InvoicePriceUnitSummaryDto(
                g.Key,
                g.Sum(x => x.TotalUnpaid),
                g.Count(),
                $"بدهکار: {g.Count(x => x.HasDebt)} فاکتور | معوقه: {g.Count(x => x.IsOverdue)}"
            ))
            .OrderByDescending(x => x.Amount)
            .ToList();

        if (unpaidGroups.Count == 0)
        {
            unpaidGroups = [new InvoicePriceUnitSummaryDto("تومان", 0, 0, "هیچ مانده مطالبات تسویه‌نشده‌ای وجود ندارد")];
        }

        // 2. Today's Sell Invoices by PriceUnit
        var todaySellGroups = calculated
            .Where(x => x.InvoiceType == InvoiceType.Sell && x.InvoiceDate == today)
            .GroupBy(x => x.PriceUnitTitle)
            .Select(g => new InvoicePriceUnitSummaryDto(
                g.Key,
                g.Sum(x => x.TotalAmount),
                g.Count(),
                $"امروز: {g.Count()} فاکتور فروش"
            ))
            .ToList();

        if (todaySellGroups.Count == 0)
        {
            todaySellGroups = [new InvoicePriceUnitSummaryDto("تومان", 0, 0, "امروز فاکتور فروشی ثبت نشده است")];
        }

        // 3. Today's Purchase Invoices by PriceUnit
        var todayPurchaseGroups = calculated
            .Where(x => x.InvoiceType == InvoiceType.Purchase && x.InvoiceDate == today)
            .GroupBy(x => x.PriceUnitTitle)
            .Select(g => new InvoicePriceUnitSummaryDto(
                g.Key,
                g.Sum(x => x.TotalAmount),
                g.Count(),
                $"امروز: {g.Count()} فاکتور خرید"
            ))
            .ToList();

        if (todayPurchaseGroups.Count == 0)
        {
            todayPurchaseGroups = [new InvoicePriceUnitSummaryDto("تومان", 0, 0, "امروز فاکتور خریدی ثبت نشده است")];
        }

        return new InvoiceOverviewStatsResponse(
            totalCount,
            sellCount,
            purchaseCount,
            paidCount,
            debtCount,
            overdueCount,
            Math.Round(averageValue, 0),
            unpaidGroups,
            todaySellGroups,
            todayPurchaseGroups
        );
    }
}