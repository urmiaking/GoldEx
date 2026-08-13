using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GoldEx.Client.Pages.Home.Components;

public partial class RecentInvoicesOverview : IAsyncDisposable
{
    [Inject] private IInvoiceService InvoiceService { get; set; } = default!;

    private List<GetInvoiceListResponse> _invoices = [];
    private int _totalInvoicesCount;

    // Multi-PriceUnit Carousels State
    private List<InvoicePriceUnitSummary> _remainingSummaries = [];
    private List<InvoicePriceUnitSummary> _todaySellSummaries = [];
    private List<InvoicePriceUnitSummary> _todayPurchaseSummaries = [];

    private int _remainingIndex;
    private int _todaySellIndex;
    private int _todayPurchaseIndex;

    private Timer? _carouselTimer;

    private int TotalInvoicesCount => _totalInvoicesCount > 0 ? _totalInvoicesCount : _invoices.Count;
    private int SellCount => _invoices.Count(x => x.InvoiceType == InvoiceType.Sell);
    private int PurchaseCount => _invoices.Count(x => x.InvoiceType == InvoiceType.Purchase);
    private int PaidCount => _invoices.Count(x => x.PaymentStatus == InvoicePaymentStatus.Paid);
    private int DebtCount => _invoices.Count(x => x.PaymentStatus == InvoicePaymentStatus.HasDebt);
    private int OverdueCount => _invoices.Count(x => x.PaymentStatus == InvoicePaymentStatus.Overdue);

    private static DateOnly TodayDate => DateOnly.FromDateTime(DateTime.Today);

    private int TodaySellCount => _invoices.Count(x => x.InvoiceType == InvoiceType.Sell && x.InvoiceDate == TodayDate);
    private int TodayPurchaseCount => _invoices.Count(x => x.InvoiceType == InvoiceType.Purchase && x.InvoiceDate == TodayDate);

    private decimal AverageInvoiceValue => TotalInvoicesCount > 0 ? _invoices.Average(x => x.TotalAmount) : 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryInvoicesAsync();
        StartCarouselTimer();
        await base.OnInitializedAsync();
    }

    private async Task LoadSummaryInvoicesAsync()
    {
        var filter = new RequestFilter(0, 200, null, null, Sdk.Common.Definitions.SortDirection.Descending);
        var invoiceFilter = new InvoiceFilter(null, null, null, null, null);

        await SendRequestAsync<IInvoiceService, PagedList<GetInvoiceListResponse>>(
            action: (service, token) => service.GetListAsync(filter, invoiceFilter, null, token),
            afterSend: response =>
            {
                _invoices = response.Data;
                _totalInvoicesCount = response.Total;
                CalculateSummariesByPriceUnit(response.Data);
            },
            createScope: true
        );
    }

    private void CalculateSummariesByPriceUnit(List<GetInvoiceListResponse> invoices)
    {
        // 1. Outstanding Receivables Balance grouped by PriceUnit
        var unpaidInvoices = invoices.Where(x => x.TotalUnpaidAmount > 0).ToList();
        if (unpaidInvoices.Any())
        {
            _remainingSummaries = unpaidInvoices
                .GroupBy(x => x.PriceUnit ?? "تومان")
                .Select(g => new InvoicePriceUnitSummary
                {
                    PriceUnit = g.Key,
                    Amount = g.Sum(x => x.TotalUnpaidAmount),
                    Count = g.Count(),
                    Subtitle = $"بدهکار: {g.Count()} فاکتور | معوقه: {g.Count(i => i.PaymentStatus == InvoicePaymentStatus.Overdue)}"
                })
                .OrderByDescending(x => x.Amount)
                .ToList();
        }
        else
        {
            _remainingSummaries = [new InvoicePriceUnitSummary { PriceUnit = "تومان", Amount = 0, Count = 0, Subtitle = "هیچ مانده مطالبات تسویه‌نشده‌ای وجود ندارد" }];
        }

        // 2. Today's Sales Volume grouped by PriceUnit
        var todaySellInvoices = invoices.Where(x => x.InvoiceType == InvoiceType.Sell && x.InvoiceDate == TodayDate).ToList();
        if (todaySellInvoices.Any())
        {
            _todaySellSummaries = todaySellInvoices
                .GroupBy(x => x.PriceUnit ?? "تومان")
                .Select(g => new InvoicePriceUnitSummary
                {
                    PriceUnit = g.Key,
                    Amount = g.Sum(x => x.TotalAmount),
                    Count = g.Count(),
                    Subtitle = $"امروز: {g.Count()} فاکتور فروش"
                })
                .ToList();
        }
        else
        {
            _todaySellSummaries = [new InvoicePriceUnitSummary { PriceUnit = "تومان", Amount = 0, Count = 0, Subtitle = "امروز فاکتور فروشی ثبت نشده است" }];
        }

        // 3. Today's Purchase Volume grouped by PriceUnit
        var todayPurchaseInvoices = invoices.Where(x => x.InvoiceType == InvoiceType.Purchase && x.InvoiceDate == TodayDate).ToList();
        if (todayPurchaseInvoices.Any())
        {
            _todayPurchaseSummaries = todayPurchaseInvoices
                .GroupBy(x => x.PriceUnit ?? "تومان")
                .Select(g => new InvoicePriceUnitSummary
                {
                    PriceUnit = g.Key,
                    Amount = g.Sum(x => x.TotalAmount),
                    Count = g.Count(),
                    Subtitle = $"امروز: {g.Count()} فاکتور خرید"
                })
                .ToList();
        }
        else
        {
            _todayPurchaseSummaries = [new InvoicePriceUnitSummary { PriceUnit = "تومان", Amount = 0, Count = 0, Subtitle = "امروز فاکتور خریدی ثبت نشده است" }];
        }
    }

    private void StartCarouselTimer()
    {
        StopCarouselTimer();
        _carouselTimer = new Timer(4000); // 4 seconds
        _carouselTimer.Elapsed += OnCarouselTimerElapsed;
        _carouselTimer.AutoReset = true;
        _carouselTimer.Start();
    }

    private void StopCarouselTimer()
    {
        _carouselTimer?.Stop();
        _carouselTimer?.Dispose();
        _carouselTimer = null;
    }

    private async void OnCarouselTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await InvokeAsync(() =>
        {
            if (_remainingSummaries.Count > 1) _remainingIndex = (_remainingIndex + 1) % _remainingSummaries.Count;
            if (_todaySellSummaries.Count > 1) _todaySellIndex = (_todaySellIndex + 1) % _todaySellSummaries.Count;
            if (_todayPurchaseSummaries.Count > 1) _todayPurchaseIndex = (_todayPurchaseIndex + 1) % _todayPurchaseSummaries.Count;

            StateHasChanged();
        });
    }

    private void ToggleRemainingIndex()
    {
        if (_remainingSummaries.Count > 1)
        {
            _remainingIndex = (_remainingIndex + 1) % _remainingSummaries.Count;
            StateHasChanged();
        }
    }

    private void ToggleTodaySellIndex()
    {
        if (_todaySellSummaries.Count > 1)
        {
            _todaySellIndex = (_todaySellIndex + 1) % _todaySellSummaries.Count;
            StateHasChanged();
        }
    }

    private void ToggleTodayPurchaseIndex()
    {
        if (_todayPurchaseSummaries.Count > 1)
        {
            _todayPurchaseIndex = (_todayPurchaseIndex + 1) % _todayPurchaseSummaries.Count;
            StateHasChanged();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        StopCarouselTimer();
        await base.DisposeAsync();
    }
}

public class InvoicePriceUnitSummary
{
    public string PriceUnit { get; set; } = default!;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public string Subtitle { get; set; } = default!;
}
