using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class CustomerBalanceReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid? PriceUnitId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? Type { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public decimal? MinimumThreshold { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? SearchQuery { get; set; }

    public string? TitleFilter => _filter.PriceUnitId.HasValue ? $"({_priceUnit?.Title})" : null;

    private readonly int _version = new Random().Next(0, 1000);
    private CustomerBalanceFilterVm _filter = default!;
    private List<CustomerRemainingBalanceRpResponse>? _items;
    private GetPriceUnitResponse? _priceUnit;
    private ReportSummaryVm? _summary;

    protected override void OnInitialized()
    {
        TransactionType? type = null;

        if (!string.IsNullOrWhiteSpace(Type) &&
            Enum.TryParse<TransactionType>(Type, ignoreCase: true, out var parsed))
        {
            type = parsed;
        }

        _filter = new CustomerBalanceFilterVm
        {
            PriceUnitId = PriceUnitId,
            MinimumThreshold = MinimumThreshold,
            SearchQuery = SearchQuery,
            Type = type,
            DateRange = FromDate.HasValue || ToDate.HasValue
                ? new DateRange(FromDate, ToDate)
                : null
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadPriceUnitAsync();
        await LoadReportAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPriceUnitAsync()
    {
        if (PriceUnitId is null)
            return;

        await SendRequestAsync<IPriceUnitService, GetPriceUnitResponse>(
            action: (s, ct) => s.GetAsync(PriceUnitId.Value, ct),
            afterSend: response => _priceUnit = response,
            createScope: true);
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<CustomerRemainingBalanceRpResponse>>(
            action: (s, ct) => s.GetCustomerRemainingBalanceAsync(request, ct),
            createScope: true);

        CalculateSummary();
    }

    private void CalculateSummary()
    {
        if (_items == null || !_items.Any())
        {
            _summary = null;
            return;
        }

        var groupedByPriceUnit = _items
            .GroupBy(x => x.PriceUnitTitle)
            .Select(g => new CustomerBalanceFilterVm.PriceUnitSummary
            {
                PriceUnitTitle = g.Key,
                TotalPayable = g.Sum(x => x.PayableAmount),
                TotalReceivable = g.Sum(x => x.ReceivableAmount)
            })
            .ToList();

        // Create a section for each price unit  
        var sections = groupedByPriceUnit.Select(pu => new SummarySection
        {
            Title = pu.PriceUnitTitle,
            Items =
                [
                    new SummaryItem
                    {
                        Label = "مجموع پرداختنی",
                        Value = pu.TotalPayable.ToCurrencyFormat(pu.PriceUnitTitle),
                        Type = "credit",
                        ShowIcon = true,
                        IconType = "credit-icon"
                    },
                    new SummaryItem
                    {
                        Label = "مجموع دریافتنی",
                        Value = pu.TotalReceivable.ToCurrencyFormat(pu.PriceUnitTitle),
                        Type = "debit",
                        ShowIcon = true,
                        IconType = "debit-icon"
                    },
                    new SummaryItem
                    {
                        Label = "خالص",
                        Value = $"{Math.Abs(pu.TotalReceivable - pu.TotalPayable).ToCurrencyFormat(pu.PriceUnitTitle)} {(pu.TotalReceivable >= pu.TotalPayable ? "بدهکار" : "بستانکار")}",
                        Type = "net",
                        ShowIcon = true,
                        IconType = pu.TotalReceivable >= pu.TotalPayable ? "positive-icon" : "negative-icon"
                    }
                ]
        }).ToList();

        _summary = new ReportSummaryVm
        {
            Sections = sections
        };
    }

    private decimal GetNet(CustomerRemainingBalanceRpResponse x)
        => x.ReceivableAmount - x.PayableAmount;

    private string GetTransactionClass(CustomerRemainingBalanceRpResponse item)
    {
        var net = GetNet(item);
        if (net == 0) return string.Empty;

        return net > 0
            ? "debit"
            : "credit";
    }

    private string GetTransactionIconClass(CustomerRemainingBalanceRpResponse item)
    {
        var net = GetNet(item);
        if (net == 0) 
            return string.Empty;

        return net > 0 ? "debit-icon" : "credit-icon";
    }

    private string GetNetAmount(CustomerRemainingBalanceRpResponse x)
    {
        var net = GetNet(x);
        var amountString = Math.Abs(net).ToCurrencyFormat(x.PriceUnitTitle);

        return net > 0 ? $"{amountString} بدهکار"
            : net < 0 ? $"{amountString} بستانکار"
            : $"{0m.ToCurrencyFormat(x.PriceUnitTitle)}";
    }

    private static string GetAmount(decimal amount, string priceUnit)
    {
        return amount is 0 ? "-" : Math.Abs(amount).ToCurrencyFormat(priceUnit);
    }
}