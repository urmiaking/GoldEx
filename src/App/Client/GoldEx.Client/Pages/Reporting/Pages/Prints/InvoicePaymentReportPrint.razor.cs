using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class InvoicePaymentReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public string InvoiceType { get; set; } = default!;

    [Parameter, SupplyParameterFromQuery]
    public long InvoiceNumber { get; set; }

    public string TitleFilter => $"{_invoiceType?.GetDisplayName()} شماره {InvoiceNumber}";

    private readonly int _version = new Random().Next(0, 1000);
    private InvoicePaymentFilterVm _filter = default!;
    private List<InvoicePaymentRpResponse>? _items;
    private ReportSummaryVm? _summary;
    private InvoiceType? _invoiceType;

    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(InvoiceType) &&
            Enum.TryParse<InvoiceType>(InvoiceType, ignoreCase: true, out var parsed))
        {
            _invoiceType = parsed;
        }

        _filter = new InvoicePaymentFilterVm
        {
            InvoiceType = _invoiceType,
            InvoiceNumber = InvoiceNumber,
            DateRange = FromDate.HasValue || ToDate.HasValue
                ? new DateRange(FromDate, ToDate)
                : null
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadReportAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<InvoicePaymentRpResponse>>(
            action: (s, ct) => s.GetInvoicePaymentsAsync(request, ct),
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

        var totalPaid = _items
            .Where(x => x.PaymentSide == PaymentSide.Pay)
            .Sum(x => x.Amount * (x.ExchangeRate ?? 1));

        var totalReceived = _items
            .Where(x => x.PaymentSide == PaymentSide.Receive)
            .Sum(x => x.Amount * (x.ExchangeRate ?? 1));

        var invoiceRemainingPrice = _items.First().InvoiceRemainingPrice;
        var priceUnit = _items.First().InvoicePriceUnit;

        _summary = new ReportSummaryVm
        {
            Sections = 
            [
                new SummarySection
                {
                    Items = 
                    [
                        new SummaryItem
                        {
                            Label = "مجموع پرداختی ها",
                            Value = totalPaid.ToCurrencyFormat(priceUnit),
                            Type = "debit",
                            ShowIcon = true,
                            IconType = "debit-icon"
                        },
                        new SummaryItem
                        {
                            Label = "مجموع دریافتی ها",
                            Value = totalReceived.ToCurrencyFormat(priceUnit),
                            Type = "credit",
                            ShowIcon = true,
                            IconType = "credit-icon"
                        },
                        new SummaryItem
                        {
                            Label = "مانده فاکتور",
                            Value = Math.Abs(invoiceRemainingPrice).ToCurrencyFormat(priceUnit),
                            Type = invoiceRemainingPrice switch
                            {
                                0 => "net",
                                > 0 => "debit",
                                < 0 => "credit"
                            },
                            ShowIcon = true,
                            IconType = invoiceRemainingPrice switch
                            {
                                0 => "",
                                > 0 => "debit-icon",
                                < 0 => "credit-icon"
                            }
                        }
                    ]
                }
            ]
        };
    }
}