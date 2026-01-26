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

public partial class PurchaseInvoiceReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid? PriceUnitId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public Guid? CustomerId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public string? PaymentStatus { get; set; }

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
    private PurchaseInvoiceFilterVm _filter = default!;
    private List<PurchaseInvoiceRpResponse>? _items;
    private GetPriceUnitResponse? _priceUnit;
    private GetCustomerResponse? _customer;
    private ReportSummaryVm? _summary;

    protected override void OnInitialized()
    {
        InvoicePaymentStatus? status = null;

        if (!string.IsNullOrWhiteSpace(PaymentStatus) &&
            Enum.TryParse<InvoicePaymentStatus>(PaymentStatus, ignoreCase: true, out var parsed))
        {
            status = parsed;
        }

        _filter = new PurchaseInvoiceFilterVm
        {
            PriceUnitId = PriceUnitId,
            CustomerId = CustomerId,
            PaymentStatus = status,
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

        _items = await SendRequestAsync<IReportingService, List<PurchaseInvoiceRpResponse>>(
            action: (s, ct) => s.GetPurchaseInvoicesAsync(request, ct),
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
            .Select(g => new PurchaseInvoiceFilterVm.PriceUnitSummary
            {
                PriceUnitTitle = g.Key,
                TotalPrice = g.Sum(x => x.TotalPrice),
                TotalRemaining = g.Sum(x => x.RemainingPrice)
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
                    Label = "مجموع ارزش کل",
                    Value = pu.TotalPrice.ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = "net"
                },
                new SummaryItem
                {
                    Label = "مجموع مانده",
                    Value = Math.Abs(pu.TotalRemaining).ToCurrencyFormat(pu.PriceUnitTitle),
                    Type = pu.TotalRemaining switch
                    {
                        0 => "net",
                        > 0 => "credit",
                        < 0 => "debit"
                    }
                }
            ]
        }).ToList();

        _summary = new ReportSummaryVm
        {
            Sections = sections
        };
    }

    private static string GetRemainingAmount(decimal amount, string priceUnit)
    {
        var amountString = Math.Abs(amount).ToCurrencyFormat(priceUnit);

        return amount > 0 ? $"{amountString} بستانکار"
            : amount < 0 ? $"{amountString} بدهکار"
            : "تسویه";
    }

    private string GetRemainingClass(decimal amount)
    {
        if (amount == 0) return string.Empty;

        return amount > 0
            ? "debit"
            : "credit";
    }

    private string GetRemainingIconClass(decimal amount)
    {
        if (amount == 0)
            return string.Empty;

        return amount > 0 ? "credit-icon" : "debit-icon";
    }
}