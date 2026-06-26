using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class UsedGoldHiddenProfitReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid? CustomerId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public Guid? PriceUnitId { get; set; }

    public string TitleFilter
    {
        get
        {
            var customerName = _customer?.FullName;
            var priceUnit = _priceUnit?.Title;

            var output = $"{customerName}";

            if (string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(priceUnit))
                output = priceUnit;
            else if (!string.IsNullOrEmpty(output) && !string.IsNullOrEmpty(priceUnit))
                output += $" ({priceUnit})";

            return string.IsNullOrEmpty(output) ? "همه مشتریان" : output;
        }
    }

    private readonly int _version = new Random().Next(0, 1000);
    private UsedGoldHiddenProfitFilterVm _filter = default!;
    private List<UsedGoldHiddenProfitRpResponse>? _items;
    private GetPriceUnitResponse? _priceUnit;
    private GetCustomerResponse? _customer;
    private ReportSummaryVm? _summary;

    protected override void OnInitialized()
    {
        _filter = new UsedGoldHiddenProfitFilterVm
        {
            CustomerId = CustomerId,
            PriceUnitId = PriceUnitId,
            DateRange = FromDate.HasValue || ToDate.HasValue
                ? new DateRange(FromDate, ToDate)
                : null
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomerAsync();
        await LoadPriceUnitAsync();
        await LoadReportAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCustomerAsync()
    {
        if (CustomerId is null)
            return;

        await SendRequestAsync<ICustomerService, GetCustomerResponse>(
            action: (s, ct) => s.GetAsync(CustomerId.Value, ct),
            afterSend: response => _customer = response,
            createScope: true);
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

        _items = await SendRequestAsync<IReportingService, List<UsedGoldHiddenProfitRpResponse>>(
            action: (s, ct) => s.GetUsedGoldHiddenProfitAsync(request, ct),
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

        var priceUnitSummaries = _items
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

        var sections = priceUnitSummaries.Select(pu => new SummarySection
        {
            Title = pu.PriceUnitTitle,
            Items =
            [
                new SummaryItem
                {
                    Label = "مجموع وزن کارکرده",
                    Value = pu.TotalWeight.ToWeightFormat(GoldUnitType.Gram),
                    Type = "net"
                },
                new SummaryItem
                {
                    Label = "مجموع پرداختی",
                    Value = pu.TotalPaidAmount.ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "net"
                },
                new SummaryItem
                {
                    Label = "مجموع ارزش واقعی طلا",
                    Value = pu.TotalRealValue.ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "net"
                },
                new SummaryItem
                {
                    Label = "مجموع سود پنهان",
                    Value = pu.TotalHiddenProfit.ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "credit"
                }
            ]
        }).ToList();

        _summary = new ReportSummaryVm
        {
            Sections = sections
        };
    }
}
