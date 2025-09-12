using GoldEx.Client.Pages.Calculate.Validators;
using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class SimpleCalculator
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;

    private CalculatorVm _model = new();
    private MudForm _form = default!;
    private readonly CalculatorValidator _calculatorValidator = new();

    private Timer? _timer;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(30);
    private GetSettingResponse? _settings;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];

    private decimal? _rawPrice;
    private decimal? _wage;
    private decimal? _profit;
    private decimal? _tax;
    private decimal? _finalPrice;
    private string? _barcode;
    private string? _barcodeFieldHelperText;

    private bool _applySafetyMargin = true;
    private bool _wageFieldMenuOpen;
    private bool _weightFieldMenuOpen;

    private string? WageFieldAdornmentText => _model.WageType switch
    {
        WageType.Percent => WageType.Percent.GetDisplayName(),
        WageType.Fixed => _model.WagePriceUnit?.Title,
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(_model.WageType), _model.WageType, null)
    };
    private string? WageTypeAdornmentIcon =>
        _model.WageType switch
        {
            WageType.Percent => Icons.Material.Filled.Percent,
            WageType.Fixed => Icons.Material.Filled.Money,
            null => Icons.Material.Filled.MoneyOff,
            _ => throw new ArgumentOutOfRangeException(nameof(_model.WageType), _model.WageType, null)
        };
    private string? WageExchangeRateLabel =>
        _model is { WageType: WageType.Fixed, WagePriceUnit: not null, PriceUnit: not null }
            ? $"نرخ تبدیل {_model.WagePriceUnit.Title} به {_model.PriceUnit.Title}"
            : null;

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            await LoadPriceUnitsAsync();
            await LoadSettingsAsync();
            await StartTimer();
        }
        finally
        {
            StateHasChanged();
        }
        await base.OnParametersSetAsync();
    }

    #region Load Initial Data

    private async Task LoadPriceUnitsAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authenticationState.User.Identity is { IsAuthenticated: false })
        {
            var priceUnit = new GetPriceUnitTitleResponse(Guid.Empty, "ریال", false, true, false);

            _model.PriceUnit = priceUnit;
            _model.WagePriceUnit = priceUnit;

            await OnPriceUnitChanged(_model.PriceUnit);

            return;
        }

        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: async response =>
            {
                _priceUnits = response;

                if (_model.PriceUnit is null)
                {
                    _model.PriceUnit = response.FirstOrDefault(x => x.IsDefault);
                    await OnPriceUnitChanged(_model.PriceUnit);
                }

                _model.WagePriceUnit ??= response.FirstOrDefault(x => x.IsDefault);
            });
    }

    private async Task LoadSettingsAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authenticationState.User.Identity is { IsAuthenticated: false })
            return;

        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response =>
            {
                _settings = response;

                if (_settings?.PriceUpdateInterval > TimeSpan.Zero)
                {
                    _updateInterval = _settings.PriceUpdateInterval;
                }

                _model.ProfitPercent = _settings?.GoldProfitPercent ?? 7;
                _model.TaxPercent = _settings?.TaxPercent ?? 10;
            });
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(_model.GoldUnitType, _model.PriceUnit?.Id, _applySafetyMargin, ct),
            afterSend: response =>
            {
                decimal.TryParse(response?.Value, out var gramPriceValue);

                _model.GramPrice = gramPriceValue;

                StateHasChanged();
            });
    }

    #endregion

    private async Task Calculate()
    {
        try
        {
            if (_model.Weight == 0 || _model is { WageType: not null, Wage: null })
            {
                ResetCalculations();
                return;
            }

            await _form.Validate();

            if (_form.IsValid)
            {
                _rawPrice = CalculatorHelper.Product.CalculateRawPrice(_model.Weight, _model.GramPrice, _model.Fineness, 1, _model.ProductType);
                _wage = CalculatorHelper.Product.CalculateWage(_rawPrice.Value, _model.Weight, _model.Wage, _model.WageType, _model.ExchangeRate);
                _profit = CalculatorHelper.Product.CalculateProfit(_rawPrice.Value, _wage.Value, _model.ProductType, _model.ProfitPercent);
                _tax = CalculatorHelper.Product.CalculateTax(_wage.Value, _profit.Value, _model.TaxPercent, _model.ProductType);
                _finalPrice = CalculatorHelper.Product.CalculateFinalPrice(_rawPrice.Value, _wage.Value, _profit.Value, _tax.Value, _model.ExtraCosts, _model.ProductType);
            }
            else
            {
                ResetCalculations();
            }
        }
        finally
        {
            StateHasChanged();
        }
    }

    #region OnChanged

    private async Task OnWageTypeChanged(WageType? wageType)
    {
        _model.WageType = wageType;
        switch (wageType)
        {
            case WageType.Percent:
                _model.ExchangeRate = null;
                break;
            case WageType.Fixed:
                if (_model.WagePriceUnit != null)
                    await SelectWagePriceUnit(_model.WagePriceUnit);
                break;
            case null:
                _model.Wage = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }

        await Calculate();
    }

    private async void OnWageChanged(decimal? wage)
    {
        _model.Wage = wage;

        await Calculate();
    }

    private void OnWageAdornmentClicked()
    {
        if (_model.WageType is WageType.Fixed)
        {
            _wageFieldMenuOpen = !_wageFieldMenuOpen;
        }
    }

    private void OnWeightAdornmentClicked()
    {
        _weightFieldMenuOpen = !_weightFieldMenuOpen;
    }

    private async Task SelectWagePriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        try
        {
            _model.WagePriceUnit = priceUnit;

            if (_model.PriceUnit is null)
                return;

            if (_model.PriceUnit != _model.WagePriceUnit && _model.WagePriceUnit != null)
                await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                    action: (s, ct) => s.GetExchangeRateAsync(_model.WagePriceUnit.Id, _model.PriceUnit.Id, ct),
                    afterSend: response =>
                    {
                        if (response.ExchangeRate.HasValue)
                        {
                            _model.ExchangeRate = response.ExchangeRate.Value;
                        }
                    });
        }
        finally
        {
            await Calculate();
        }
    }

    private async Task SelectGoldUnitType(GoldUnitType unitType)
    {
        _model.GoldUnitType = unitType;
        await LoadGramPriceAsync();
    }

    private void OnWageExchangeRateChanged(decimal? exchangeRate)
    {
        _model.ExchangeRate = exchangeRate;
    }

    private async void OnProductTypeChanged(ProductType productType)
    {
        _model.ProductType = productType;
        switch (productType)
        {
            case ProductType.Jewelry:
                _model.ProfitPercent = _settings?.JewelryProfitPercent ?? 20;
                _model.Fineness = 750m;
                _applySafetyMargin = true;
                break;
            case ProductType.Gold:
                _model.ProfitPercent = _settings?.GoldProfitPercent ?? 7;
                _model.Fineness = 750m;
                _applySafetyMargin = true;
                break;
            case ProductType.MoltenGold:
                _model.Fineness = 750m;
                _model.ProfitPercent = _settings?.MoltenGoldCommissionPercent ?? 1.5m;
                _model.WageType = null;
                _model.Wage = null;
                _applySafetyMargin = true;
                break;
            case ProductType.UsedGold:
                _applySafetyMargin = false;
                _model.Fineness = (int?)_settings?.UsedGoldFineness ?? 735;
                _model.Wage = null;
                _model.WageType = null;
                _model.ProfitPercent = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
        await LoadGramPriceAsync();
        await Calculate();
    }

    private async void OnFinenessChanged(decimal fineness)
    {
        _model.Fineness = fineness;

        await Calculate();
    }

    private async void OnGramPriceChanged(decimal gramPrice)
    {
        _model.GramPrice = gramPrice;

        await Calculate();
    }

    private async void OnWeightChanged(decimal weight)
    {
        _model.Weight = weight;

        await Calculate();
    }

    private async void OnProfitChanged(decimal profit)
    {
        _model.ProfitPercent = profit;

        await Calculate();
    }

    private async void OnExtraCostChanges(decimal? additionalPrices)
    {
        _model.ExtraCosts = additionalPrices;

        await Calculate();
    }

    private async Task OnPriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        _model.PriceUnit = priceUnit;

        await LoadGramPriceAsync();

        if (_model.WagePriceUnit != null)
            await SelectWagePriceUnit(_model.WagePriceUnit);

        await Calculate();

        StateHasChanged();
    }

    private async Task OnBarcodeChanged(string barcode)
    {
        try
        {
            _barcode = barcode;

            if (string.IsNullOrWhiteSpace(barcode))
            {
                OnBarcodeCleared();
                return;
            }

            await SendRequestAsync<IProductService, GetProductResponse?>(
                 action: async (s, ct) => await s.GetAsync(barcode, ct),
                 afterSend: async response =>
                 {
                     if (response is null)
                         return;

                     _barcodeFieldHelperText = response.Name;

                     var wagePriceUnit = _priceUnits.FirstOrDefault(x => x.Id == response.WagePriceUnitId);

                     _model = CalculatorVm.CreateFrom(response, _model, wagePriceUnit);
                     await SelectWagePriceUnit(_model.WagePriceUnit!);
                     OnProductTypeChanged(_model.ProductType);
                 },
                 cancelPrevious: true);
        }
        finally
        {
            await Calculate();
        }
    }

    private void OnBarcodeCleared()
    {
        _barcode = null;
        _barcodeFieldHelperText = null;
        ResetModel();
        ResetCalculations();
    }

    private async void ResetModel()
    {
        _model.Weight = 0;
        _model.Fineness = 750m;
        _model.ProductType = ProductType.Gold;
        _model.Wage = 0;
        _model.WageType = null;
        _model.ExtraCosts = null;
        _model.ProfitPercent = _settings?.GoldProfitPercent ?? 7;

        _applySafetyMargin = true;
        await LoadGramPriceAsync();
    }

    private void ResetCalculations()
    {
        _rawPrice = null;
        _wage = null;
        _profit = null;
        _tax = null;
        _finalPrice = null;
    }

    #endregion

    #region Timer

    private Task StartTimer()
    {
        _timer = new Timer(
            TimerCallback,
            null,
            TimeSpan.FromSeconds(0),
            _updateInterval
        );

        return Task.CompletedTask;
    }

    private async void TimerCallback(object? state)
    {
        if (IsDisposed)
            return;

        await InvokeAsync(async () =>
        {
            if (IsDisposed) return;

            await LoadGramPriceAsync();
            await Calculate();
        });
    }

    // Your local dispose logic now calls the base implementation
    public override async ValueTask DisposeAsync()
    {
        if (_timer is not null) await _timer.DisposeAsync();

        await base.DisposeAsync();
    }

    #endregion
}