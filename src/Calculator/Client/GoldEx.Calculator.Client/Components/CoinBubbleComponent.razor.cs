using GoldEx.Calculator.Client.Services;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Calculator.Client.Components;

public partial class CoinBubbleComponent : IAsyncDisposable
{
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? Class { get; set; }

    [Inject] private IPriceStateService PriceStateService { get; set; } = default!;
    [Inject] private CalculationHistoryStore HistoryStore { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private readonly CoinBubbleVm _model = new();

    protected override async Task OnInitializedAsync()
    {
        PriceStateService.OnPricesUpdated += HandlePricesUpdated;
        await LoadLiveRatesAsync();
        await base.OnInitializedAsync();
    }

    private async void HandlePricesUpdated()
    {
        if (IsDisposed) return;
        await InvokeAsync(async () =>
        {
            if (IsDisposed) return;
            await LoadLiveRatesAsync();
        });
    }

    private async Task LoadLiveRatesAsync()
    {
        try
        {
            var prices = await PriceStateService.GetListAsync();

            // ۱. دریافت دقیق نرخ انس جهانی طلا (دلار) - تفکیک دقیق از انس نقره، پلاتین و پالادیوم
            var goldOunce = prices.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.Gold)
                         ?? prices.FirstOrDefault(x => x.Title == "انس جهانی طلا" || x.Title == "انس طلا" || x.Title == "انس جهانی")
                         ?? prices.FirstOrDefault(x => x.Title.Contains("انس") && (x.Title.Contains("طلا") || x.Title.Contains("جهانی") || x.Title.Contains("Gold")) && !x.Title.Contains("نقره") && !x.Title.Contains("پلاتین") && !x.Title.Contains("پالادیوم"))
                         ?? prices.FirstOrDefault(x => x.Type == MarketType.Ounce && !x.Title.Contains("نقره") && !x.Title.Contains("پلاتین") && !x.Title.Contains("پالادیوم"));

            if (goldOunce != null && TryParseDecimal(goldOunce.Value, out var ounceVal) && ounceVal > 0)
            {
                _model.OuncePriceUsd = NormalizeOuncePrice(ounceVal);
            }
            else if (_model.OuncePriceUsd == 0)
            {
                _model.OuncePriceUsd = 2735m;
            }

            // ۲. دریافت نرخ دلار آزاد (تومان)
            var usd = prices.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.UsDollar)
                   ?? prices.FirstOrDefault(x => x.Title == "دلار" || x.Title == "دلار آزاد" || x.Title == "دلار آمریکا")
                   ?? prices.FirstOrDefault(x => x.Title.Contains("دلار") && !x.Title.Contains("کانادا") && !x.Title.Contains("استرالیا") && !x.Title.Contains("نیما") && !x.Title.Contains("حواله") && !x.Title.Contains("مبادله") && !x.Title.Contains("توافقی"));

            if (usd != null && TryParseDecimal(usd.Value, out var usdVal) && usdVal > 0)
            {
                _model.UsdRateToman = NormalizeUsdRate(usdVal);
            }
            else if (_model.UsdRateToman == 0)
            {
                _model.UsdRateToman = 94500m;
            }

            // ۳. دریافت داینامیک قیمت انواع سکه از دیتای زنده API (قیمت‌ها در GetListAsync از قبل بر حسب تومان هستند)
            var matchedCoins = new List<CoinItemModel>();

            // سکه امامی (طرح جدید)
            var emamiPrice = FindCoinPrice(prices, PriceCatalog.SekehEmami, "سکه امامی", "طرح جدید", "امامی");
            matchedCoins.Add(new CoinItemModel
            {
                Type = CoinType.Emami,
                Title = "تمام سکه امامی (طرح جدید)",
                WeightGrams = 8.133m,
                Fineness = 900m,
                MarketPrice = emamiPrice > 0 ? emamiPrice : 54200000m
            });

            // سکه بهار آزادی (طرح قدیم)
            var baharPrice = FindCoinPrice(prices, PriceCatalog.SekehBaharAzadi, "سکه بهار آزادی", "طرح قدیم", "بهار آزادی");
            matchedCoins.Add(new CoinItemModel
            {
                Type = CoinType.BaharAzadi,
                Title = "تمام سکه بهار آزادی (طرح قدیم)",
                WeightGrams = 8.133m,
                Fineness = 900m,
                MarketPrice = baharPrice > 0 ? baharPrice : 51300000m
            });

            // نیم سکه بهار آزادی
            var nimPrice = FindCoinPrice(prices, PriceCatalog.NimSeke, "نیم سکه", "نیم");
            matchedCoins.Add(new CoinItemModel
            {
                Type = CoinType.Nim,
                Title = "نیم سکه بهار آزادی",
                WeightGrams = 4.066m,
                Fineness = 900m,
                MarketPrice = nimPrice > 0 ? nimPrice : 28100000m
            });

            // ربع سکه بهار آزادی
            var robPrice = FindCoinPrice(prices, PriceCatalog.RobSeke, "ربع سکه", "ربع");
            matchedCoins.Add(new CoinItemModel
            {
                Type = CoinType.Rob,
                Title = "ربع سکه بهار آزادی",
                WeightGrams = 2.033m,
                Fineness = 900m,
                MarketPrice = robPrice > 0 ? robPrice : 18200000m
            });

            // سکه یک گرمی بانک مرکزی
            var geramiPrice = FindCoinPrice(prices, PriceCatalog.SekehYekGerami, "سکه یک گرمی", "سکه گرمی");
            matchedCoins.Add(new CoinItemModel
            {
                Type = CoinType.Gerami,
                Title = "سکه یک گرمی (بانک مرکزی)",
                WeightGrams = 1.000m,
                Fineness = 900m,
                MarketPrice = geramiPrice > 0 ? geramiPrice : 8500000m
            });

            _model.Coins = matchedCoins;
            StateHasChanged();
        }
        catch
        {
            // fallback gracefully
        }
    }

    private static decimal FindCoinPrice(List<GetPriceResponse> prices, PriceCatalog targetCatalog, params string[] keywords)
    {
        // ابتدا جستجو بر اساس شناسه کاتالوگ دقیق
        var match = prices.FirstOrDefault(x => x.PriceCatalog == targetCatalog);
        if (match == null)
        {
            // در غیر این صورت جستجو بر اساس عنوان با اولویت عناوینی که کلمه «حباب» ندارند
            match = prices.FirstOrDefault(x => !x.Title.Contains("حباب") && keywords.Any(k => x.Title.Equals(k, StringComparison.OrdinalIgnoreCase)))
                 ?? prices.FirstOrDefault(x => !x.Title.Contains("حباب") && keywords.Any(k => x.Title.Contains(k, StringComparison.OrdinalIgnoreCase)));
        }

        if (match != null && TryParseDecimal(match.Value, out var val) && val > 0)
        {
            // در سرویس سرور، قیمت‌ها قبلاً به تومان تبدیل شده‌اند؛ بنابراین مستقیماً استفاده می‌شود
            return val;
        }

        return 0;
    }

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var cleaned = text.Replace(",", "").Replace("٬", "").Trim();
        return decimal.TryParse(cleaned, out value);
    }

    private static decimal NormalizeOuncePrice(decimal rawVal)
    {
        if (rawVal <= 0) return 0;
        if (rawVal > 100_000)
        {
            return 2735m;
        }
        return rawVal;
    }

    private static decimal NormalizeUsdRate(decimal rawVal)
    {
        if (rawVal <= 0) return 0;
        // اگر نرخ دلار هنوز به ریال باشد (بالای ۵۰۰,۰۰۰ ریال)، به تومان تبدیل می‌شود
        if (rawVal >= 500_000)
        {
            return rawVal / 10;
        }
        return rawVal;
    }

    private static string GetBubbleBadgeClass(decimal bubblePct)
    {
        if (bubblePct <= 10m) return "bubble-badge-fair";
        if (bubblePct <= 25m) return "bubble-badge-warning";
        return "bubble-badge-danger";
    }

    private async Task SaveCoinToHistoryAsync(CoinItemModel coin, decimal intrinsic, decimal bubbleAmount, decimal bubblePct)
    {
        await HistoryStore.AddAsync(new CalculationHistoryItem
        {
            Title = $"حباب {coin.Title}",
            Category = "حباب سکه",
            SummaryText = $"بازار: {coin.MarketPrice:N0} | ذاتی: {intrinsic:N0} تومان",
            ResultValue = $"{(bubblePct > 0 ? "+" : "")}{bubblePct:F1}%",
            Unit = "حباب",
            Details = new Dictionary<string, string>
            {
                ["قیمت بازار"] = $"{coin.MarketPrice:N0} تومان",
                ["ارزش ذاتی"] = $"{intrinsic:N0} تومان",
                ["مبلغ حباب"] = $"{bubbleAmount:N0} تومان",
                ["درصد حباب"] = $"{bubblePct:F1}%"
            }
        });

        Snackbar.Add($"محاسبه حباب {coin.Title} در تاریخچه ثبت شد", Severity.Success);
    }

    public override async ValueTask DisposeAsync()
    {
        PriceStateService.OnPricesUpdated -= HandlePricesUpdated;
        await base.DisposeAsync();
    }
}
