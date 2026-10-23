using GoldEx.Server.Domain.Reporting;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Utilities;

public static class ProfitAnalysisBuilder
{
    public static ProfitAnalysisRpResponse Build(IReadOnlyList<GoldProfitSource> history,
        IReadOnlySet<Guid> otherMovements, ProfitAnalysisRpRequest request)
    {
        var from = request.FromDate.HasValue ? DateOnly.FromDateTime(request.FromDate.Value) : DateOnly.MinValue;
        var to = request.ToDate.HasValue ? DateOnly.FromDateTime(request.ToDate.Value) : DateOnly.MaxValue;
        var rows = new List<ProfitAnalysisRow>();
        foreach (var product in history.GroupBy(x => x.ProductId))
        {
            var purchases = product.Where(x => x.InvoiceType == InvoiceType.Purchase).ToList();
            decimal consumedWeight = 0;
            foreach (var sale in product.Where(x => x.InvoiceType == InvoiceType.Sell)
                .OrderBy(x => x.Date).ThenBy(x => x.CreatedAt).ThenBy(x => x.ItemId))
            {
                consumedWeight += sale.Weight;
                if (sale.Date < from || sale.Date > to ||
                    (request.CustomerId.HasValue && request.CustomerId.Value != sale.CustomerId) ||
                    (request.CategoryId.HasValue && request.CategoryId != sale.CategoryId) ||
                    (request.ProductType.HasValue && request.ProductType != sale.ProductType)) continue;

                var origins = purchases.Where(p => p.Date < sale.Date ||
                    (p.Date == sale.Date && p.CreatedAt <= sale.CreatedAt)).ToList();
                var purchase = origins.Count == 1 ? origins[0] : null;
                string? reason = null;
                GoldProfitResult? result = null;
                if (sale.IsInstant || otherMovements.Contains(sale.ProductId))
                    reason = "ورود دستی، برگشت یا گردش غیر از خرید و فروش؛ منشأ کالا نیاز به بررسی دارد.";
                else if (origins.Count != 1)
                    reason = origins.Count == 0 ? "سند خرید پیش از فروش پیدا نشد." : "بیش از یک منشأ خرید وجود دارد؛ تخصیص بهای خرید مشخص نیست.";
                else if (consumedWeight > purchase!.Weight + 0.0000001m)
                    reason = "جمع وزن فروش از وزن خرید مشخص‌شده بیشتر است.";
                else
                    result = GoldProfitCalculator.Calculate(sale, purchase!, out reason);

                rows.Add(new(sale.InvoiceId, sale.InvoiceNumber, sale.Date, sale.CustomerName,
                    sale.ProductName, sale.Barcode, sale.GramPrice > 0 ? sale.RawAmount / sale.GramPrice : 0,
                    purchase?.InvoiceId, purchase?.InvoiceNumber, purchase?.Date,
                    sale.BasePriceUnitId, sale.BasePriceUnit,
                    result?.NetSales, result?.PurchaseCost, result?.GrossProfit, result?.GoldPriceEffect,
                    result?.ExchangeRateEffect, result?.TradingMargin, reason));
            }
        }

        var calculated = rows.Where(r => r.IncompleteReason is null).ToList();
        var summaries = calculated.GroupBy(r => new { r.PriceUnitId, r.PriceUnit }).Select(g =>
            new ProfitAnalysisSummary(g.Key.PriceUnitId, g.Key.PriceUnit, g.Count(),
                g.Sum(x => x.NetSales!.Value), g.Sum(x => x.PurchaseCost!.Value), g.Sum(x => x.GrossProfit!.Value),
                g.Sum(x => x.GoldPriceEffect!.Value), g.Sum(x => x.ExchangeRateEffect!.Value), g.Sum(x => x.TradingMargin!.Value)))
            .OrderBy(g => g.PriceUnit).ToList();
        var items = request.SummaryOnly ? [] : calculated.OrderByDescending(r => r.InvoiceDate)
            .ThenByDescending(r => r.InvoiceNumber).ThenBy(r => r.Barcode)
            .ToList();
        return new(summaries, items, rows.Count, calculated.Count, rows.Count - calculated.Count);
    }
}
