using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.Helpers;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class ProfitAnalysisReportPrint
{
    private static string Money(decimal? amount, string unit) => amount?.ToCurrencyFormat(unit) ?? "—";
    private ReportSummaryVm? PrintSummary => Data is null ? null : new()
    {
        Sections = Data.Summaries.Select(unit => new SummarySection
        {
            Title = unit.PriceUnit,
            Info = $"{unit.Count} ردیف محاسبه‌شده",
            Items = new[]
            {
                ("فروش خالص", unit.NetSales), ("بهای خرید", unit.PurchaseCost),
                ("سود ناخالص", unit.GrossProfit), ("اثر نرخ طلا", unit.GoldPriceEffect),
                ("اثر ارز", unit.ExchangeRateEffect), ("حاشیه معامله", unit.TradingMargin)
            }.Select(item => new SummaryItem
            {
                Label = item.Item1, Value = item.Item2.ToCurrencyFormat(unit.PriceUnit), Type = "net", ShowIcon = false
            }).ToList()
        }).ToList()
    };
}
