using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class PurchaseInvoiceReportView
{
    [Parameter] public List<PurchaseInvoiceRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private PurchaseInvoiceFilterVm.PurchaseInvoiceReportSummary? _summary;

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
            .GroupBy(x => x.PriceUnit)
            .Select(g => new PurchaseInvoiceFilterVm.PriceUnitSummary
            {
                PriceUnitTitle = g.Key,
                TotalPrice = g.Sum(x => x.TotalPrice),
                TotalRemaining = g.Sum(x => x.RemainingPrice)
            })
            .OrderBy(x => x.PriceUnitTitle)
            .ToList();

        _summary = new PurchaseInvoiceFilterVm.PurchaseInvoiceReportSummary
        {
            PriceUnitSummaries = priceUnitSummaries
        };
    }

    private string GetAmount(PurchaseInvoiceRpResponse context)
    {
        var remainingAmount = context.RemainingPrice;

        return remainingAmount is 0 ? "تسویه" : Math.Abs(remainingAmount).ToCurrencyFormat(context.PriceUnit);
    }

    private Color GetAmountColor(PurchaseInvoiceRpResponse context)
    {
        var remainingAmount = context.RemainingPrice;
        return remainingAmount switch
        {
            0 => Color.Default,
            > 0 => Color.Success,
            < 0 => Color.Error
        };
    }

    private void OnViewInvoice(Guid id)
    {
        Navigation.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id }));
    }
}