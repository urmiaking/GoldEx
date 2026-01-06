using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GoldEx.Client.Pages.Reporting.ViewModels.CustomerBalanceFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class CustomerBalanceReportView
{
    [Parameter] public List<CustomerRemainingBalanceRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private CustomerBalanceReportSummary? _summary;

    protected override void OnParametersSet()
    {
        CalculateSummary();
        base.OnParametersSet();
    }

    private void CalculateSummary()
    {
        if (Items == null || !Items.Any())
        {
            _summary = null;
            return;
        }

        var groupedByPriceUnit = Items
            .GroupBy(x => x.PriceUnitTitle)
            .Select(g => new CustomerBalanceFilterVm.PriceUnitSummary
            {
                PriceUnitTitle = g.Key,
                TotalPayable = g.Sum(x => x.PayableAmount),
                TotalReceivable = g.Sum(x => x.ReceivableAmount)
            })
            .ToList();

        _summary = new CustomerBalanceReportSummary
        {
            PriceUnitSummaries = groupedByPriceUnit,
        };
    }

    private decimal GetNet(CustomerRemainingBalanceRpResponse x)
        => x.ReceivableAmount - x.PayableAmount;

    private string? GetNetIcon(CustomerRemainingBalanceRpResponse x)
    {
        var net = GetNet(x);
        if (net == 0) return null;

        return net > 0
            ? Icons.Material.Filled.ArrowDownward 
            : Icons.Material.Filled.ArrowUpward;
    }

    private Color GetNetColor(CustomerRemainingBalanceRpResponse x)
    {
        var net = GetNet(x);
        if (net == 0) return Color.Default;

        return net > 0
            ? Color.Error
            : Color.Success;
    }

    private string GetNetAmount(CustomerRemainingBalanceRpResponse x)
    {
        var net = GetNet(x);
        var amountString = Math.Abs(net).ToCurrencyFormat(x.PriceUnitTitle);

        return amountString;

        //return net > 0 ? $"{amountString} بدهکار"
        //    : net < 0 ? $"{amountString} بستانکار"
        //    : $"{0m.ToCurrencyFormat(x.PriceUnitTitle)}";
    }

    private static string GetAmount(decimal amount, string priceUnit)
    {
        return amount is 0 ? "-" : Math.Abs(amount).ToCurrencyFormat(priceUnit);
    }
}