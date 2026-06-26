using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class UsedGoldHiddenProfitReportView
{
    [Parameter] public List<UsedGoldHiddenProfitRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private UsedGoldHiddenProfitFilterVm.UsedGoldHiddenProfitReportSummary? _summary;

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

        var priceUnitSummaries = Items
            .GroupBy(x => x.PriceUnitTitle)
            .Select(g => new UsedGoldHiddenProfitFilterVm.PriceUnitSummary
            {
                PriceUnitTitle = g.Key,
                TotalWeight = g.Sum(x => x.Weight * x.Quantity),
                TotalPaidAmount = g.Sum(x => x.PaidAmount),
                TotalRealValue = g.Sum(x => x.RealValue),
                TotalHiddenProfit = g.Sum(x => x.HiddenProfit)
            })
            .OrderBy(x => x.PriceUnitTitle)
            .ToList();

        _summary = new UsedGoldHiddenProfitFilterVm.UsedGoldHiddenProfitReportSummary
        {
            PriceUnitSummaries = priceUnitSummaries
        };
    }

    private void OnViewInvoice(Guid id)
    {
        Navigation.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id }));
    }
}
