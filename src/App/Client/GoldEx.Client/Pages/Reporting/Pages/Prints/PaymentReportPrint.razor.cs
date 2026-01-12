using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class PaymentReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid? PriceUnitId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public Guid? CustomerId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? PaymentSide { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? PaymentType { get; set; }

    public string TitleFilter
    {
        get
        {
            var customerName = _customer?.FullName;
            var priceUnit = _priceUnit?.Title;
            
            var output = $"{customerName}";

            if (!string.IsNullOrEmpty(priceUnit))
                output += $" ({priceUnit})";

            return output;
        }
    }

    private readonly int _version = new Random().Next(0, 1000);
    private PaymentFilterVm _filter = default!;
    private List<PaymentRpResponse>? _items;
    private GetPriceUnitResponse? _priceUnit;
    private GetCustomerResponse? _customer;
    private ReportSummaryVm? _summary;

    protected override void OnInitialized()
    {
        PaymentSide? paymentSide = null;
        PaymentType? paymentType = null;

        if (!string.IsNullOrWhiteSpace(PaymentSide) &&
            Enum.TryParse<PaymentSide>(PaymentSide, ignoreCase: true, out var parsedSide))
        {
            paymentSide = parsedSide;
        }

        if (!string.IsNullOrWhiteSpace(PaymentType) &&
            Enum.TryParse<PaymentType>(PaymentType, ignoreCase: true, out var parsedType))
        {
            paymentType = parsedType;
        }

        _filter = new PaymentFilterVm
        {
            PriceUnitId = PriceUnitId,
            CustomerId = CustomerId,
            PaymentSide = paymentSide,
            PaymentType = paymentType,
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

        _items = await SendRequestAsync<IReportingService, List<PaymentRpResponse>>(
            action: (s, ct) => s.GetPaymentsAsync(request, ct),
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
            .GroupBy(x => x.PriceUnit)
            .Select(g => new PaymentFilterVm.PriceUnitSummary
            {
                PriceUnitTitle = g.Key,
                TotalPaid = g.Where(x => x.PaymentSide == Shared.Enums.PaymentSide.Pay).Sum(x => x.Amount),
                TotalReceived = g.Where(x => x.PaymentSide == Shared.Enums.PaymentSide.Receive).Sum(x => x.Amount),
                AvgExchangeRate = g.Average(x => x.ExchangeRate)
            })
            .OrderBy(x => x.PriceUnitTitle)
            .ToList();

        var sections = groupedByPriceUnit.Select(pu => new SummarySection
        {
            Title = pu.PriceUnitTitle,
            Items =
            [
                new SummaryItem
                {
                    Label = "مجموع پرداختی ها",
                    Value = pu.TotalPaid.ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "debit",
                    ShowIcon = true,
                    IconType = "debit-icon"
                },
                new SummaryItem
                {
                    Label = "مجموع دریافتی ها",
                    Value = pu.TotalReceived.ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "credit",
                    ShowIcon = true,
                    IconType = "credit-icon"
                },
                new SummaryItem
                {
                    Label = "خالص",
                    Value = Math.Abs(pu.NetAmount).ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "net"
                }
            ]
        }).ToList();

        _summary = new ReportSummaryVm
        {
            Sections = sections
        };
    }
}