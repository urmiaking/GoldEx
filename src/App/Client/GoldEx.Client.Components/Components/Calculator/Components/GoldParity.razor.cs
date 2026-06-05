using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Components.Calculator.Components;

public partial class GoldParity
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public int Elevation { get; set; } = 24;

    private Timer? _timer;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(30);
    private bool _isManualMode;
    private string _currencyUnit = "تومان";
    private decimal _gramPerMesghal = 4.6083m;

    // Live rates cached from API
    private decimal _liveOuncePrice;
    private decimal _liveUsdRate;
    private decimal _liveAedRate;
    private decimal _liveTryRate;
    private decimal _liveGold18Price;
    private decimal _liveGold24Price;
    private decimal _liveMesghalPrice;

    // User overrides (used when _isManualMode is true)
    private decimal _manualOuncePrice;
    private decimal _manualUsdRate;
    private decimal _manualAedRate;
    private decimal _manualTryRate;
    private decimal _manualGold18Price;
    private decimal _manualGold24Price;
    private decimal _manualMesghalPrice;

    // Current values to use for all calculations (takes manual or live depending on mode)
    private decimal OuncePrice => _isManualMode ? _manualOuncePrice : _liveOuncePrice;
    private decimal UsdRate => _isManualMode ? _manualUsdRate : _liveUsdRate;
    private decimal AedRate => _isManualMode ? _manualAedRate : _liveAedRate;
    private decimal TryRate => _isManualMode ? _manualTryRate : _liveTryRate;
    private decimal Gold18Price => _isManualMode ? _manualGold18Price : _liveGold18Price;
    private decimal Gold24Price => _isManualMode ? _manualGold24Price : _liveGold24Price;
    private decimal MesghalPrice => _isManualMode ? _manualMesghalPrice : _liveMesghalPrice;

    // Calculation Constants
    private const decimal TroyOunceGrams = 31.1034768m;
    private const decimal AedUsdPeg = 3.6725m; // 1 USD = 3.6725 AED

    protected override async Task OnInitializedAsync()
    {
        await LoadSettingsAsync();
        await LoadPricesAsync();
        await StartTimer();
        await base.OnInitializedAsync();
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response =>
            {
                if (response is not null)
                {
                    if (response.PriceUpdateInterval > TimeSpan.Zero)
                    {
                        _updateInterval = response.PriceUpdateInterval;
                    }
                    if (response.GramPerMesghal > 0)
                    {
                        _gramPerMesghal = response.GramPerMesghal;
                    }
                }
            },
            createScope: true
        );
    }

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceService, List<GetPriceResponse>>(
            action: (s, ct) => s.GetListAsync(null, ct),
            afterSend: response =>
            {
                if (!response.Any()) return;

                // Extract prices by PriceCatalog enum instead of title strings
                var ounceRes = response.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.Gold);
                var usdRes = response.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.UsDollar);
                var aedRes = response.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.UaeDeram);
                var tryRes = response.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.TurkeyLira);

                var g18Res = response.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.Geram18);
                var g24Res = response.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.Geram24);
                var mesghalRes = response.FirstOrDefault(x => x.PriceCatalog == PriceCatalog.Mazanne);

                // Update unit name from one of the domestic responses
                if (g18Res != null)
                {
                    _currencyUnit = g18Res.Unit;
                }

                _liveOuncePrice = ParsePriceValue(ounceRes?.Value);
                _liveUsdRate = ParsePriceValue(usdRes?.Value);
                _liveAedRate = ParsePriceValue(aedRes?.Value);
                _liveTryRate = ParsePriceValue(tryRes?.Value);

                _liveGold18Price = ParsePriceValue(g18Res?.Value);
                _liveGold24Price = ParsePriceValue(g24Res?.Value);
                _liveMesghalPrice = ParsePriceValue(mesghalRes?.Value);

                // If not in manual mode, initialize manual properties to match live rates
                if (!_isManualMode)
                {
                    ResetManualRates();
                }

                StateHasChanged();
            },
            createScope: true
        );
    }

    private decimal ParsePriceValue(string? valueStr)
    {
        if (string.IsNullOrWhiteSpace(valueStr)) return 0;
        var cleanValue = valueStr.Replace(",", "").Replace("٬", "").Trim();
        return decimal.TryParse(cleanValue, out var val) ? val : 0;
    }

    private void ResetManualRates()
    {
        _manualOuncePrice = _liveOuncePrice;
        _manualUsdRate = _liveUsdRate;
        _manualAedRate = _liveAedRate;
        _manualTryRate = _liveTryRate;
        _manualGold18Price = _liveGold18Price;
        _manualGold24Price = _liveGold24Price;
        _manualMesghalPrice = _liveMesghalPrice;
    }

    private void ToggleManualMode(bool value)
    {
        _isManualMode = value;
        if (_isManualMode)
        {
            ResetManualRates();
        }
    }

    #region Timer

    private Task StartTimer()
    {
        _timer = new Timer(
            TimerCallback,
            null,
            _updateInterval,
            _updateInterval
        );

        return Task.CompletedTask;
    }

    private void TimerCallback(object? state)
    {
        _ = InvokeAsync(async () =>
        {
            if (IsDisposed) return;
            await LoadPricesAsync();
        });
    }

    public override async ValueTask DisposeAsync()
    {
        if (_timer is not null)
        {
            await _timer.DisposeAsync();
        }

        await base.DisposeAsync();
    }

    #endregion

    #region Calculation Helpers

    // 18k Parities
    private decimal Usd18kParity => UsdRate > 0 ? (OuncePrice * UsdRate * 0.750m) / TroyOunceGrams : 0m;
    private decimal Aed18kParity => AedRate > 0 ? (OuncePrice * AedUsdPeg * AedRate * 0.750m) / TroyOunceGrams : 0m;
    private decimal Try18kParity
    {
        get
        {
            if (TryRate <= 0 || UsdRate <= 0) return 0m;
            var crossRate = UsdRate / TryRate;
            return (OuncePrice * crossRate * TryRate * 0.750m) / TroyOunceGrams;
        }
    }

    // 24k Parities
    private decimal Usd24kParity => UsdRate > 0 ? (OuncePrice * UsdRate) / TroyOunceGrams : 0m;
    private decimal Aed24kParity => AedRate > 0 ? (OuncePrice * AedUsdPeg * AedRate) / TroyOunceGrams : 0m;
    private decimal Try24kParity
    {
        get
        {
            if (TryRate <= 0 || UsdRate <= 0) return 0m;
            var crossRate = UsdRate / TryRate;
            return (OuncePrice * crossRate * TryRate) / TroyOunceGrams;
        }
    }

    // Mithqal Parities (Mazanne) - 4.6083 grams of 17k (705 fineness)
    private decimal UsdMesghalParity => UsdRate > 0 ? (OuncePrice * UsdRate * _gramPerMesghal * 0.705m) / TroyOunceGrams : 0m;
    private decimal AedMesghalParity => AedRate > 0 ? (OuncePrice * AedUsdPeg * AedRate * _gramPerMesghal * 0.705m) / TroyOunceGrams : 0m;
    private decimal TryMesghalParity
    {
        get
        {
            if (TryRate <= 0 || UsdRate <= 0) return 0m;
            var crossRate = UsdRate / TryRate;
            return (OuncePrice * crossRate * TryRate * _gramPerMesghal * 0.705m) / TroyOunceGrams;
        }
    }

    // Intermediate rates
    private decimal AedUsdCrossRate => AedUsdPeg;
    private decimal TryUsdCrossRate => TryRate > 0 ? (UsdRate / TryRate) : 0m;

    // Currency gold ounces
    private decimal OunceInUsd => OuncePrice;
    private decimal OunceInAed => OuncePrice * AedUsdPeg;
    private decimal OunceInTry => OuncePrice * TryUsdCrossRate;

    // Implied USD Rates (Toman Dollar rate)
    private decimal ImpliedUsdFromAed => AedRate * AedUsdPeg;

    // Signal logic
    public (string Text, string ColorClass, string Icon) GetSignal(decimal bubblePercent)
    {
        if (bubblePercent <= -1.0m)
        {
            return ("فرصت خرید مناسب", "text-success font-weight-bold d-flex align-center gap-1", MudBlazor.Icons.Material.Filled.TrendingUp);
        }
        else if (bubblePercent >= 1.0m)
        {
            return ("ریسک خرید بالا", "text-danger font-weight-bold d-flex align-center gap-1", MudBlazor.Icons.Material.Filled.TrendingDown);
        }
        else
        {
            return ("عادی / خنثی", "text-info font-weight-bold d-flex align-center gap-1", MudBlazor.Icons.Material.Filled.TrendingFlat);
        }
    }

    #endregion
}
