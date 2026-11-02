using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
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
    private List<CustomerRemainingBalanceRpResponse>? _lastItems;
    private List<CustomerRemainingBalanceRpResponse> _orderedItems = [];
    private ICollection<CustomerRemainingBalanceRpResponse> OrderedItems => _orderedItems;
    private string _sort = "name";
    private bool _descending;
    private void UpdateOrderedItems()
    {
        if (Items is null) { _orderedItems = []; return; }
        IEnumerable<CustomerRemainingBalanceRpResponse> sorted = _sort switch
        {
            "payable" => _descending ? Items.OrderByDescending(x => x.PayableAmount) : Items.OrderBy(x => x.PayableAmount),
            "receivable" => _descending ? Items.OrderByDescending(x => x.ReceivableAmount) : Items.OrderBy(x => x.ReceivableAmount),
            "net" => _descending ? Items.OrderByDescending(GetNet) : Items.OrderBy(GetNet),
            _ => _descending ? Items.OrderByDescending(x => x.CustomerName) : Items.OrderBy(x => x.CustomerName)
        };
        _orderedItems = sorted.ToList();
    }
    private string SortIndicator(string field) => field == _sort ? (_descending ? "↓" : "↑") : "";
    private void SetSort(string field)
    {
        _descending = field == _sort ? !_descending : field != "name";
        _sort = field;
        UpdateOrderedItems();
    }
    private static string AmountClass(decimal amount) => amount > 0 ? "is-debit" : amount < 0 ? "is-credit" : "";
    private string NetClass(CustomerRemainingBalanceRpResponse item) => GetNet(item) > 0 ? "is-debit" : GetNet(item) < 0 ? "is-credit" : "";


    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(Items, _lastItems))
        {
            _lastItems = Items;
            UpdateOrderedItems();
        }
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
            .Select(g => new PriceUnitSummary
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
        => x.ReceivableAmount + x.PayableAmount;

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
    }

    private static string GetAmount(decimal amount, string priceUnit)
    {
        return amount is 0 ? "-" : Math.Abs(amount).ToCurrencyFormat(priceUnit);
    }

    private void OnViewSource(CustomerRemainingBalanceRpResponse x)
    {
        Navigation.NavigateTo(ClientRoutes.Reporting.CustomerTransactions.AppendQueryString(new { x.CustomerId, x.PriceUnitId }));
    }

    private static Color GetAmountColor(decimal amount)
    {
        return amount > 0 ? Color.Error : Color.Success;
    }
}