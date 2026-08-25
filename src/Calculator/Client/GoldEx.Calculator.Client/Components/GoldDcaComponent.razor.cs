using GoldEx.Calculator.Client.Services;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Calculator.Client.Components;

public partial class GoldDcaComponent : IAsyncDisposable
{
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? Class { get; set; }

    [Inject] private IPriceStateService PriceStateService { get; set; } = default!;
    [Inject] private CalculationHistoryStore HistoryStore { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private readonly GoldDcaVm _model = new();

    protected override async Task OnInitializedAsync()
    {
        PriceStateService.OnPricesUpdated += HandlePricesUpdated;
        await LoadLivePriceAsync();
        await base.OnInitializedAsync();
    }

    private async void HandlePricesUpdated()
    {
        if (IsDisposed) return;
        await InvokeAsync(async () =>
        {
            if (IsDisposed) return;
            await LoadLivePriceAsync();
        });
    }

    private async Task LoadLivePriceAsync()
    {
        try
        {
            var price = await PriceStateService.GetAsync(GoldUnitType.Gram, null, false);
            if (price != null && decimal.TryParse(price.Value?.Replace(",", "").Trim(), out var rate) && rate > 0)
            {
                _model.LiveGramPrice = rate;
                StateHasChanged();
            }
        }
        catch
        {
            // fallback gracefully
        }
    }

    private void AddPurchaseItem()
    {
        _model.Purchases.Add(new DcaPurchaseItem
        {
            Title = $"پله {_model.Purchases.Count + 1}",
            WeightGrams = 5m,
            BuyGramPrice = _model.LiveGramPrice > 0 ? _model.LiveGramPrice : 5000000m
        });
    }

    private void RemovePurchaseItem(int index)
    {
        if (index >= 0 && index < _model.Purchases.Count && _model.Purchases.Count > 1)
        {
            _model.Purchases.RemoveAt(index);
        }
    }

    private async Task SaveToHistoryAsync()
    {
        await HistoryStore.AddAsync(new CalculationHistoryItem
        {
            Title = $"پورتفوی طلا ({_model.TotalWeight:F2} گرم)",
            Category = "سرمایه‌گذاری",
            SummaryText = $"میانگین خرید: {_model.WeightedAverageBuyPrice:N0} | بازدهی: {_model.NetProfitLossPercent:F1}%",
            ResultValue = $"{(_model.NetProfitLossAmount >= 0 ? "+" : "")}{_model.NetProfitLossAmount:N0}",
            Unit = "تومان",
            Details = new Dictionary<string, string>
            {
                ["کل وزن اندوخته"] = $"{_model.TotalWeight:F3} گرم",
                ["کل سرمایه‌گذاری"] = $"{_model.TotalInvested:N0} تومان",
                ["میانگین قیمت خرید"] = $"{_model.WeightedAverageBuyPrice:N0} تومان",
                ["ارزش روز بازار"] = $"{_model.CurrentMarketValue:N0} تومان",
                ["سود / زیان خالص"] = $"{_model.NetProfitLossAmount:N0} تومان ({_model.NetProfitLossPercent:F2}%)",
                ["نقطه سر‌به‌سر"] = $"{_model.BreakEvenGramPrice:N0} تومان"
            }
        });

        Snackbar.Add("تحلیل سرمایه‌گذاری در تاریخچه ثبت شد", Severity.Success);
    }

    public override async ValueTask DisposeAsync()
    {
        PriceStateService.OnPricesUpdated -= HandlePricesUpdated;
        await base.DisposeAsync();
    }
}
