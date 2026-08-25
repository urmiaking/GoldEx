using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Calculator.Client.Components;

public partial class LivePriceTicker : IAsyncDisposable
{
    [Inject] private IPriceStateService PriceStateService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private List<TickerItem> _prices = [];
    private int _currentIndex = 0;
    private bool _isLoading = true;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;

    public record TickerItem(string Title, string FormattedValue, string Unit);

    protected override async Task OnInitializedAsync()
    {
        PriceStateService.OnPricesUpdated += HandlePricesUpdated;
        await LoadPricesAsync();

        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(3.5));
        _ = StartTimerLoopAsync(_cts.Token);

        await base.OnInitializedAsync();
    }

    private async Task StartTimerLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && _timer != null && await _timer.WaitForNextTickAsync(token))
            {
                if (IsDisposed) break;
                await InvokeAsync(() =>
                {
                    if (_prices.Count > 1)
                    {
                        _currentIndex = (_currentIndex + 1) % _prices.Count;
                        StateHasChanged();
                    }
                });
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on dispose
        }
    }

    public void NextPrice()
    {
        if (_prices.Count > 1)
        {
            _currentIndex = (_currentIndex + 1) % _prices.Count;
            StateHasChanged();
        }
    }

    public void PrevPrice()
    {
        if (_prices.Count > 1)
        {
            _currentIndex = (_currentIndex - 1 + _prices.Count) % _prices.Count;
            StateHasChanged();
        }
    }

    private async void HandlePricesUpdated()
    {
        if (IsDisposed) return;
        await InvokeAsync(async () =>
        {
            if (IsDisposed) return;
            await LoadPricesAsync();
        });
    }

    private async Task LoadPricesAsync()
    {
        try
        {
            _isLoading = true;
            // فقط قیمت آیتم‌هایی که IsPinned = true است دریافت می‌شوند
            var list = await PriceStateService.GetListAsync(isPinned: true);
            var items = new List<TickerItem>();

            if (list.Count > 0)
            {
                foreach (var p in list)
                {
                    var (val, unit) = NormalizePrice(p.Title, p.Value, p.Unit);
                    if (!string.IsNullOrWhiteSpace(val) && val != "0" && val != "-")
                    {
                        items.Add(new TickerItem(CleanTitle(p.Title), val, unit));
                    }
                }
            }

            // اگر لیست خالی بود، نرخ‌های تکی طلا و مظنه را استعلام کن
            if (items.Count == 0)
            {
                var gram18 = await PriceStateService.GetAsync(GoldUnitType.Gram, null, false);
                if (gram18 != null)
                {
                    var (val, unit) = NormalizePrice("طلای ۱۸", gram18.Value, gram18.Unit);
                    items.Add(new TickerItem("طلای ۱۸", val, unit));
                }

                var mesghal = await PriceStateService.GetAsync(GoldUnitType.Mesghal, null, false);
                if (mesghal != null)
                {
                    var (val, unit) = NormalizePrice("آبشده", mesghal.Value, mesghal.Unit);
                    items.Add(new TickerItem("آبشده", val, unit));
                }
            }

            _prices = items;
            if (_currentIndex >= _prices.Count)
            {
                _currentIndex = 0;
            }
        }
        catch
        {
            // fallback gracefully
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private static string CleanTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "طلا";
        if (title.Contains("18") || title.Contains("۱۸")) return "طلای ۱۸";
        if (title.Contains("24") || title.Contains("۲۴")) return "طلای ۲۴";
        if (title.Contains("مظنه") || title.Contains("آبشده") || title.Contains("مثقال")) return "آبشده";
        if (title.Contains("امامی")) return "سکه امامی";
        if (title.Contains("بهار آزادی") || title.Contains("طرح قدیم")) return "بهار آزادی";
        if (title.Contains("نیم")) return "نیم سکه";
        if (title.Contains("ربع")) return "ربع سکه";
        if (title.Contains("گرمی")) return "سکه گرمی";
        if (title.Contains("انس") || title.Contains("Ounce")) return "انس طلا";
        if (title.Contains("دلار")) return "دلار";
        if (title.Contains("یورو")) return "یورو";
        if (title.Contains("درهم")) return "درهم";
        if (title.Contains("پوند")) return "پوند";
        return title.Replace("(گرم)", "").Replace("نقدی", "").Trim();
    }

    private static (string formattedVal, string unit) NormalizePrice(string title, string? rawVal, string? rawUnit)
    {
        if (string.IsNullOrWhiteSpace(rawVal))
            return ("-", "تومان");

        var cleaned = rawVal.Replace(",", "").Replace("٬", "").Trim();
        if (!decimal.TryParse(cleaned, out var num))
            return (rawVal, "تومان");

        var isOunce = title.Contains("انس") || title.Contains("Ounce");
        if (isOunce)
        {
            return ($"{num:N0}", "دلار");
        }

        // تبدیل هوشمند ریال به تومان
        if (title.Contains("سکه"))
        {
            if (num >= 100_000_000 || (title.Contains("گرمی") && num >= 40_000_000))
            {
                num /= 10;
            }
        }
        else if (title.Contains("دلار") || title.Contains("یورو") || title.Contains("درهم"))
        {
            if (num >= 500_000)
            {
                num /= 10;
            }
        }
        else
        {
            // طلا، مظنه و سایر اقلام
            if ((rawUnit != null && rawUnit.Contains("ریال")) || num >= 100_000_000)
            {
                num /= 10;
            }
        }

        return ($"{num:N0}", "تومان");
    }

    public async Task RefreshAsync()
    {
        await PriceStateService.RefreshAsync();
        await LoadPricesAsync();
        Snackbar.Add("قیمت‌های لحظه‌ای به‌روزرسانی شدند", Severity.Success);
    }

    public override async ValueTask DisposeAsync()
    {
        PriceStateService.OnPricesUpdated -= HandlePricesUpdated;
        _cts?.Cancel();
        _cts?.Dispose();
        _timer?.Dispose();
        await base.DisposeAsync();
    }
}
