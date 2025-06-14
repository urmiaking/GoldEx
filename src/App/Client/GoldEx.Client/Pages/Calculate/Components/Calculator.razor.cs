using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Calculate.Validators;
using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class Calculator
{
    private CalculatorVm _model = new();
    private MudForm _from = default!;
    private CalculatorValidator _calculatorValidator = new();
    private MudSelect<WageType?> _wageTypeField = default!;
    private MudNumericField<decimal?> _wageField = default!;
    private MudNumericField<decimal> _profitField = default!;

    private string? _wageFieldAdornmentText = "درصد";
    private string? _gramPriceAdornmentText;
    private string? _extraCostsAdornmentText;
    private bool _wageFieldMenuOpen;

    private decimal? _rawPrice;
    private decimal? _wage;
    private decimal? _profit;
    private decimal? _tax;
    private decimal? _finalPrice;
    private string? _barcode;
    private string? _barcodeFieldHelperText;
    private Timer? _timer;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(30);
    private GetSettingResponse? _settings;
    private GetPriceResponse? _gramPrice;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];

    private string? _wageExchangeRateLabel;

    private bool _isInitialLoading = true;
    private bool _isBarcodeProcessing = false;
    private bool _isRecalculating = false;

    protected override async Task OnParametersSetAsync()
    {
        _isInitialLoading = true;
        try
        {
            await LoadPriceUnitsAsync();
            await LoadSettingsAsync();
            await StartTimer();
        }
        finally
        {
            _isInitialLoading = false;
            StateHasChanged();
        }
        await base.OnParametersSetAsync();
    }

    #region Load Initial Data

    private async Task LoadPriceUnitsAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authenticationState.User.Identity is { IsAuthenticated: false })
            return;

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
                UpdateWageFields();
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
                _model.TaxPercent = _settings?.TaxPercent ?? 9;
            });
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(UnitType.Gold18K, _model.PriceUnit?.Id, ct),
            afterSend: response =>
            {
                _gramPrice = response;

                decimal.TryParse(response?.Value, out var gramPriceValue);

                _model.GramPrice = gramPriceValue;

                _gramPriceAdornmentText = response?.Unit;
            });
    }

    #endregion

    private async Task Calculate()
    {
        _isRecalculating = true;

        try
        {
            if (_model.Weight == 0 || _model is { WageType: not null, Wage: null })
            {
                ResetCalculations();
                return;
            }

            await _from.Validate();

            if (_from.IsValid)
            {
                _rawPrice = CalculatorHelper.CalculateRawPrice(_model.Weight, _model.GramPrice, _model.CaratType, _model.ProductType);
                _wage = CalculatorHelper.CalculateWage(_rawPrice.Value, _model.Weight, _model.Wage, _model.WageType, _model.ExchangeRate);
                _profit = CalculatorHelper.CalculateProfit(_rawPrice.Value, _wage.Value, _model.ProductType, _model.ProfitPercent);
                _tax = CalculatorHelper.CalculateTax(_wage.Value, _profit.Value, _model.TaxPercent, _model.ProductType);
                _finalPrice = CalculatorHelper.CalculateFinalPrice(_rawPrice.Value, _wage.Value, _profit.Value, _tax.Value, _model.ExtraCosts, _model.ProductType);
            }
            else
            {
                ResetCalculations();
            }
        }
        finally
        {
            _isRecalculating = false;
        }
    }

    #region OnChanged

    private async Task OnWageTypeChanged(WageType? wageType)
    {
        _model.WageType = wageType;
        switch (wageType)
        {
            case WageType.Percent:
                UpdateWageFields();
                _wageTypeField.AdornmentIcon = Icons.Material.Filled.Percent;
                _wageField.Disabled = false;
                break;
            case WageType.Fixed:
                UpdateWageFields();

                if (_model.WagePriceUnit != null)
                    await SelectWagePriceUnit(_model.WagePriceUnit);

                _wageTypeField.AdornmentIcon = Icons.Material.Filled.Money;
                _wageField.Disabled = false;
                break;
            case null:
                await _wageField.ResetAsync();
                _wageFieldAdornmentText = null;
                _wageField.Disabled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }

        await Calculate();
    }

    private async void OnWageChanged(decimal? wage)
    {
        _model.Wage = wage;

        UpdateWageFields();

        await Calculate();
    }

    private void OnWageAdornmentClicked()
    {
        if (_model.WageType is WageType.Fixed)
        {
            _wageFieldMenuOpen = !_wageFieldMenuOpen;
        }
    }

    private async Task SelectWagePriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        _isRecalculating = true;
        try
        {
            _model.WagePriceUnit = priceUnit;

            if (_model.PriceUnit is null)
                return;

            if (_model.PriceUnit != _model.WagePriceUnit)
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
            UpdateWageFields();
            await Calculate();

            _isRecalculating = false;
        }
    }

    private void OnWageExchangeRateChanged(decimal? exchangeRate)
    {
        _model.ExchangeRate = exchangeRate;

        UpdateWageFields();
    }

    private async void UpdateWageFields()
    {
        if (_model.WageType is WageType.Fixed)
        {
            _wageFieldAdornmentText = _model.WagePriceUnit?.Title;

            if (_model.WagePriceUnit?.Id != _model.PriceUnit?.Id)
            {
                _wageExchangeRateLabel = $"نرخ تبدیل {_model.WagePriceUnit?.Title} به {_model.PriceUnit?.Title}";
            }
        }
        else
        {
            _wageFieldAdornmentText = "درصد";
            _wageExchangeRateLabel = null;
            _model.ExchangeRate = null;
        }

        await Calculate();
    }

    private async void OnProductTypeChanged(ProductType productType)
    {
        _model.ProductType = productType;
        switch (productType)
        {
            case ProductType.Jewelry:
                _model.ProfitPercent = _settings?.JewelryProfitPercent ?? 20;
                break;
            case ProductType.Gold:
                _model.ProfitPercent = _settings?.GoldProfitPercent ?? 7;
                break;
            case ProductType.MoltenGold:
                break;
            case ProductType.OldGold:
                await _wageField.ResetAsync();
                await _wageTypeField.ResetAsync();
                await _profitField.ResetAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }

        await Calculate();
    }

    private async void OnCaratTypeChanged(CaratType caratType)
    {
        _model.CaratType = caratType;

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

        _extraCostsAdornmentText = priceUnit?.Title;

        await LoadGramPriceAsync();

        if (_model.WagePriceUnit != null) 
            await SelectWagePriceUnit(_model.WagePriceUnit);

        UpdateWageFields();
        await Calculate();

        StateHasChanged();
    }

    private async Task OnBarcodeChanged(string barcode)
    {
        _isBarcodeProcessing = true;
        try
        {
            _barcode = barcode;

            if (string.IsNullOrWhiteSpace(barcode))
            {
                OnBarcodeCleared();
                return;
            }

            await SendRequestAsync<IProductService, GetProductResponse?>(async (s, ct) => await s.GetAsync(barcode, ct),
                 async response =>
                {
                    if (response is null)
                        return;

                    _barcodeFieldHelperText = response.Name;

                    _model.Weight = response.Weight;
                    _model.CaratType = response.CaratType;
                    _model.Wage = response.Wage;
                    _model.WageType = response.WageType;
                    _model.ProductType = response.ProductType;
                    _model.WagePriceUnit = _priceUnits.FirstOrDefault(x => x.Id == response.WagePriceUnitId);

                    UpdateWageFields();
                    await SelectWagePriceUnit(_model.WagePriceUnit!);
                    OnProductTypeChanged(_model.ProductType);
                });
        }
        finally
        {
            _isBarcodeProcessing = false;
            await Calculate();
        }
    }

    private void OnBarcodeCleared()
    {
        _isBarcodeProcessing = true;
        try
        {
            _barcode = null;
            _barcodeFieldHelperText = null;
            ResetModel();
            ResetCalculations();
        }
        finally
        {
            _isBarcodeProcessing = false;
        }
    }

    private void ResetModel()
    {
        _model.Weight = 0;
        _model.CaratType = CaratType.Eighteen;
        _model.Wage = 0;
        _model.WageType = null;
        _model.ExtraCosts = null;
        _model.ProfitPercent = _settings?.GoldProfitPercent ?? 7;
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
            _updateInterval,
            _updateInterval
        );

        return Task.CompletedTask;
    }

    private async void TimerCallback(object? state)
    {
        await LoadGramPriceAsync();
        await Calculate();
        StateHasChanged();
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }

    #endregion
}