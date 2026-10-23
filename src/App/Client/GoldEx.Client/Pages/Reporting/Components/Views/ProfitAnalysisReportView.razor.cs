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
    private bool ShowExchangeRate => Report?.Summaries.Any(x => x.ExchangeRateEffect != 0) == true;
    protected override void OnParametersSet() => _expandedItem = null;
    private void ToggleDetails(ProfitAnalysisRow item) => _expandedItem = ReferenceEquals(item, _expandedItem) ? null : item;
    private static string Money(decimal? amount, string unit) => amount?.ToCurrencyFormat(unit) ?? "—";
    private static string InvoiceUrl(long number, InvoiceType type) => $"/invoices/viewer/{number}/{type}";
}
