using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GoldEx.Client.Pages.Reporting.ViewModels.InvoicePaymentFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class InvoicePaymentReportView
{
    [Parameter] public List<InvoicePaymentRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private InvoicePaymentReportSummary? _summary;

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

        var totalPaid = Items
            .Where(x => x.PaymentSide == PaymentSide.Pay)
            .Sum(x => x.Amount * (x.ExchangeRate ?? 1));

        var totalReceived = Items
            .Where(x => x.PaymentSide == PaymentSide.Receive)
            .Sum(x => x.Amount * (x.ExchangeRate ?? 1));

        var invoiceRemainingPrice = Items.First().InvoiceRemainingPrice;
        var priceUnit = Items.First().InvoicePriceUnit;

        _summary = new InvoicePaymentReportSummary
        {
            InvoiceRemainingPrice = invoiceRemainingPrice,
            PriceUnit = priceUnit,
            TotalPaidAmount = totalPaid,
            TotalReceivedAmount = totalReceived
        };
    }

    private string GetAmount(InvoicePaymentRpResponse context) => Math.Abs(context.Amount).ToCurrencyFormat(context.PriceUnit);

    private Color GetAmountColor(InvoicePaymentRpResponse context)
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

    private string GetAmountIcon(InvoicePaymentRpResponse context)
    {
        return context.PaymentSide switch
        {
            PaymentSide.Receive => Icons.Material.Outlined.ArrowDownward,
            PaymentSide.Pay => Icons.Material.Outlined.ArrowUpward,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}