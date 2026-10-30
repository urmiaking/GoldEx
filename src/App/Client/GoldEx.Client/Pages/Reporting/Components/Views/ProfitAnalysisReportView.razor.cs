using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class ProfitAnalysisReportView
{
    [Parameter] public ProfitAnalysisRpResponse? Report { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    private ProfitAnalysisRow? _expandedItem;
    private ProfitAnalysisRpResponse? _lastReport;
    private int _pageIndex, _pageSize = 10;
    private int PageCount => Math.Max(1, ((Report?.Items.Count() ?? 0) + _pageSize - 1) / _pageSize);
    private IEnumerable<ProfitAnalysisRow> VisibleItems => Report?.Items.Skip(_pageIndex * _pageSize).Take(_pageSize) ?? [];
    private bool ShowExchangeRate => Report?.Summaries.Any(x => x.ExchangeRateEffect != 0) == true;
    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(Report, _lastReport))
        {
            _lastReport = Report;
            _pageIndex = 0;
            _expandedItem = null;
        }
    }
    private void ChangePage(int delta)
    {
        _pageIndex = Math.Clamp(_pageIndex + delta, 0, PageCount - 1);
        _expandedItem = null;
    }
    private void OnPageSizeChanged(ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var size) && size is 10 or 25 or 50)
        {
            _pageSize = size;
            _pageIndex = 0;
            _expandedItem = null;
        }
    }
    private void ToggleDetails(ProfitAnalysisRow item) => _expandedItem = ReferenceEquals(item, _expandedItem) ? null : item;
    private static string Money(decimal? amount, string unit) => amount?.ToCurrencyFormat(unit) ?? "—";
    private static string InvoiceUrl(long number, InvoiceType type) => $"/invoices/viewer/{number}/{type}";
}