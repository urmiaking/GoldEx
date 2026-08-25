using System.Globalization;
using GoldEx.Calculator.Client.Services;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Client.Components.Calculator.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Calculator.Client.Components;

public partial class TradeInCalculatorComponent : IAsyncDisposable
{
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? Class { get; set; }

    [Inject] private IPriceStateService PriceStateService { get; set; } = default!;
    [Inject] private QuickInvoiceBasketStore BasketStore { get; set; } = default!;
    [Inject] private CalculationHistoryStore HistoryStore { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private readonly TradeInVm _model = new();
    private readonly string _currencyTitle = "تومان";

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
            if (price != null && decimal.TryParse(price.Value?.Replace(",", "").Replace("٬", "").Trim(), out var rate) && rate > 0)
            {
                // تبدیل خودکار ریال به تومان در صورت نیاز
                if (price.Unit?.Contains("ریال") == true || rate > 100_000_000)
                    rate /= 10;

                if (_model.UsedGramPrice == 0) _model.UsedGramPrice = rate;
                if (_model.NewGramPrice == 0) _model.NewGramPrice = rate;
                StateHasChanged();
            }
        }
        catch
        {
            // fallback
        }
    }

    private async Task AddTradeInToBasketAsync()
    {
        if (_model.NewTotalValue <= 0 && _model.UsedTotalValue <= 0)
        {
            Snackbar.Add("لطفاً مشخصات طلا را وارد نمایید", Severity.Warning);
            return;
        }

        var pc = new PersianCalendar();
        var now = DateTime.Now;
        var nowPersian = $"{pc.GetYear(now):0000}/{pc.GetMonth(now):00}/{pc.GetDayOfMonth(now):00} - {now.Hour:00}:{now.Minute:00}";
        var rand = new Random().Next(1000, 9999);
        var invNum = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000000}{rand}";

        // ۱. افزودن طلای نو به فاکتور
        if (_model.NewTotalValue > 0)
        {
            var newGoldPayload = new QuickInvoicePayload
            {
                InvoiceNumber = invNum,
                DateTime = nowPersian,
                ProductName = _model.NewItemName ?? "طلای نو (تحویلی)",
                ProductType = "طلا",
                Weight = $"{_model.NewWeight:G29} گرم",
                Fineness = _model.NewFineness,
                GramPrice = $"{_model.NewGramPrice:N0} {_currencyTitle}",
                ProfitPercent = _model.NewProfitPercent,
                TaxPercent = _model.NewTaxPercent,
                Wage = _model.NewWage?.ToString("G29"),
                WageType = _model.NewWageType?.GetDisplayName(),
                FinalPrice = $"{_model.NewTotalValue:N0} {_currencyTitle}"
            };

            await BasketStore.AddAsync(newGoldPayload);
        }

        // ۲. افزودن طلای کهنه دریافتی به فاکتور به صورت ردیف تعویضی
        if (_model.UsedTotalValue > 0)
        {
            var usedGoldPayload = new QuickInvoicePayload
            {
                InvoiceNumber = invNum,
                DateTime = nowPersian,
                ProductName = $"{_model.UsedItemName ?? "طلای کهنه"} (کسر تعویض)",
                ProductType = "مستعمل",
                Weight = $"{_model.UsedWeight:G29} گرم",
                Fineness = _model.UsedFineness - _model.UsedFinenessDeduction,
                GramPrice = $"{_model.UsedGramPrice:N0} {_currencyTitle}",
                ProfitPercent = 0,
                TaxPercent = 0,
                Wage = "0",
                FinalPrice = $"-{_model.UsedTotalValue:N0} {_currencyTitle}"
            };

            await BasketStore.AddAsync(usedGoldPayload);
        }

        // ۳. ثبت در تاریخچه
        await HistoryStore.AddAsync(new CalculationHistoryItem
        {
            Title = "معامله تعویض طلا",
            Category = "تعویض طلا",
            SummaryText = $"نو: {_model.NewWeight:G29} گرم | کهنه: {_model.UsedWeight:G29} گرم",
            ResultValue = Math.Abs(_model.NetDifference).ToString("N0"),
            Unit = _currencyTitle,
            Details = new Dictionary<string, string>
            {
                ["ارزش طلای نو"] = $"{_model.NewTotalValue:N0} {_currencyTitle}",
                ["ارزش طلای کهنه"] = $"{_model.UsedTotalValue:N0} {_currencyTitle}",
                ["مانده تسویه"] = $"{_model.NetDifference:N0} {_currencyTitle}"
            }
        });

        Snackbar.Add("اقلام معامله تعویض با موفقیت به سبد فاکتور اضافه شدند", Severity.Success);
    }

    public override async ValueTask DisposeAsync()
    {
        PriceStateService.OnPricesUpdated -= HandlePricesUpdated;
        await base.DisposeAsync();
    }
}
