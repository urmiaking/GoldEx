using GoldEx.Shared.DTOs.Reporting;
using Microsoft.AspNetCore.Components;
using static GoldEx.Client.Pages.Reporting.ViewModels.CurrencyInventoryFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class CurrencyInventoryReportView
{
    [Parameter] public List<CurrencyInventoryRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private CurrencyInventoryReportSummary? _summary;

    protected override void OnParametersSet()
    {
        CalculateSummary();
        base.OnParametersSet();
    }

    private void CalculateSummary()
    {
        if (Items == null || Items.Count == 0)
        {
            _summary = null;
            return;
        }

        var totalCurrentAmount = Items.Sum(x => x.CurrentAmount);
        var totalSoldAmount = Items.Sum(x => x.SoldAmount);

        _summary = new CurrencyInventoryReportSummary
        {
            TotalAmount = totalCurrentAmount,
            TotalSoldAmount = totalSoldAmount,
        };
    }
}