using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
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

        var itemsQuery = baseQuery.SelectMany(inv => inv.ProductItems.Select(item => new
        {
            CategoryId = (Guid?)(item.Product != null && item.Product.ProductCategoryId != null ? item.Product.ProductCategoryId.Value.Value : null),
            CategoryTitle = (item.Product != null && item.Product.ProductCategory != null) ? item.Product.ProductCategory.Title : "سایر / بدون دسته‌بندی",
            item.TotalWeight,
            item.Quantity,
            WageAmount = item.ItemWageAmount,
            ProfitAmount = item.ItemProfitAmount,
            TaxAmount = item.ItemTaxAmount,
            FinalAmount = item.ItemFinalAmount
        }));

        if (request.CategoryId.HasValue)
        {
            itemsQuery = itemsQuery.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryTitle))
        {
            itemsQuery = itemsQuery.Where(x => x.CategoryTitle.Contains(request.CategoryTitle));
        }

        var rawAggregates = await itemsQuery
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
            .ToListAsync(cancellationToken);

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

        var itemsQuery = baseQuery.SelectMany(inv => inv.ProductItems.Select(item => new
        {
            InvoiceId = inv.Id.Value,
            InvoiceNumber = inv.InvoiceNumber,
            InvoiceDate = inv.InvoiceDate,
            CustomerName = inv.Customer != null ? inv.Customer.FullName : null,
            PriceUnit = inv.PriceUnit != null ? inv.PriceUnit.Title : "تومان",
            ProductId = item.ProductId.Value,
            ProductName = item.Product != null ? item.Product.Name : "کالای طلا",
            Barcode = item.Product != null ? item.Product.Barcode : null,
            CategoryId = (Guid?)(item.Product != null && item.Product.ProductCategoryId != null ? item.Product.ProductCategoryId.Value.Value : null),
            CategoryTitle = (item.Product != null && item.Product.ProductCategory != null) ? item.Product.ProductCategory.Title : "سایر / بدون دسته‌بندی",
            item.TotalWeight,
            item.Quantity,
            item.GramPrice,
            WageAmount = item.ItemWageAmount,
            ProfitAmount = item.ItemProfitAmount,
            TaxAmount = item.ItemTaxAmount,
            FinalAmount = item.ItemFinalAmount
        }));

        if (request.CategoryId.HasValue)
        {
            itemsQuery = itemsQuery.Where(x => x.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryTitle))
        {
            itemsQuery = itemsQuery.Where(x => x.CategoryTitle.Contains(request.CategoryTitle));
        }

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var q = request.SearchQuery.Trim();
            itemsQuery = itemsQuery.Where(x =>
                x.ProductName.Contains(q) ||
                (x.Barcode != null && x.Barcode.Contains(q)) ||
                (x.CustomerName != null && x.CustomerName.Contains(q)) ||
                x.InvoiceNumber.ToString().Contains(q));
        }

        var paged = await itemsQuery
            .OrderByDescending(x => x.InvoiceDate)
            .ThenByDescending(x => x.InvoiceNumber)
            .Skip(request.Skip)
            .Take(Math.Clamp(request.Take, 1, 500))
            .ToListAsync(cancellationToken);

        return paged.Select(x => new SoldProductItemRpResponse(
            x.InvoiceId,
            x.InvoiceNumber,
            x.InvoiceDate,
            x.CustomerName,
            x.ProductId,
            x.ProductName,
            x.Barcode,
            x.CategoryId,
            x.CategoryTitle,
            x.TotalWeight,
            x.Quantity,
            x.GramPrice,
            x.WageAmount,
            x.ProfitAmount,
            x.TaxAmount,
            x.FinalAmount,
            x.PriceUnit
        )).ToList();
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
}