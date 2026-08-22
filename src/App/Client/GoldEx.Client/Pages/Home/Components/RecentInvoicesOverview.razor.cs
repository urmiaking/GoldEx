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

    private InvoiceOverviewStatsResponse? _stats;

    // Multi-PriceUnit Carousels State
    private List<InvoicePriceUnitSummaryDto> _remainingSummaries = [];
    private List<InvoicePriceUnitSummaryDto> _todaySellSummaries = [];
    private List<InvoicePriceUnitSummaryDto> _todayPurchaseSummaries = [];

    private int _remainingIndex;
    private int _todaySellIndex;
    private int _todayPurchaseIndex;

    private Timer? _carouselTimer;

    private int TotalInvoicesCount => _stats?.TotalInvoicesCount ?? 0;
    private int SellCount => _stats?.SellCount ?? 0;
    private int PurchaseCount => _stats?.PurchaseCount ?? 0;
    private int PaidCount => _stats?.PaidCount ?? 0;
    private int DebtCount => _stats?.DebtCount ?? 0;
    private int OverdueCount => _stats?.OverdueCount ?? 0;

    private decimal AverageInvoiceValue => _stats?.AverageInvoiceValue ?? 0;

    protected override async Task OnInitializedAsync()
    {
        await LoadSummaryInvoicesAsync();
        StartCarouselTimer();
        await base.OnInitializedAsync();
    }

    private async Task LoadSummaryInvoicesAsync()
    {
        await SendRequestAsync<IInvoiceService, InvoiceOverviewStatsResponse>(
            action: (service, token) => service.GetOverviewStatsAsync(token),
            afterSend: response =>
            {
                _stats = response;
                _remainingSummaries = response.RemainingSummaries;
                _todaySellSummaries = response.TodaySellSummaries;
                _todayPurchaseSummaries = response.TodayPurchaseSummaries;
            },
            createScope: true
        );
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
