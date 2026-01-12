using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class PaymentReportView
{
    [Parameter] public List<PaymentRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private PaymentFilterVm.PaymentReportSummary? _summary;

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
            .Select(g => new PaymentFilterVm.PriceUnitSummary
            {
                PriceUnitTitle = g.Key,
                TotalReceived = g.Where(x => x.PaymentSide == PaymentSide.Receive).Sum(x => x.Amount),
                TotalPaid = g.Where(x => x.PaymentSide == PaymentSide.Pay).Sum(x => x.Amount),
                AvgExchangeRate = g.Average(x => x.ExchangeRate)
            })
            .OrderBy(x => x.PriceUnitTitle)
            .ToList();

        _summary = new PaymentFilterVm.PaymentReportSummary
        {
            PriceUnitSummaries = priceUnitSummaries
        };
    }

    private string GetAmount(PaymentRpResponse context) => Math.Abs(context.Amount).ToCurrencyFormat(context.PriceUnit);

    private Color GetAmountColor(PaymentRpResponse context)
    {
        return context.PaymentSide switch
        {
            PaymentSide.Receive => Color.Success,
            PaymentSide.Pay => Color.Error,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void OnViewInvoice(Guid id)
    {
        Navigation.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id }));
    }

    private string GetAmountIcon(PaymentRpResponse context)
    {
        return context.PaymentSide switch
        {
            PaymentSide.Receive => Icons.Material.Outlined.ArrowDownward,
            PaymentSide.Pay => Icons.Material.Outlined.ArrowUpward,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}