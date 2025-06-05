using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Calculate.Validators;
using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class Calculator
{
    private CalculatorVm _model = new();
    private MudForm _from = default!;
    private CalculatorValidator _calculatorValidator = new();
    private GetSettingResponse? _settings;
    private MudSelect<WageType?> _wageTypeField = default!;
    private MudNumericField<decimal?> _wageField = default!;
    private MudNumericField<decimal> _profitField = default!;

    private string? _wageFieldAdornmentText = "درصد";
    private decimal? _rawPrice;
    private decimal? _wage;
    private decimal? _profit;
    private decimal? _tax;
    private decimal? _finalPrice;
    private string? _barcode;
    private string? _barcodeFieldHelperText;
    private Timer? _timer;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(30);

    protected override async Task OnParametersSetAsync()
    {
        await LoadPricesAsync();
        await LoadSettingsAsync();
        await StartTimer();
        await base.OnParametersSetAsync();
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

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>((s, ct) => s.GetAsync(UnitType.Gold18K, ct),
            response =>
            {
                _model.GramPrice = response != null ? decimal.Parse(response.Value) / 10m : 0m;
            });

        await SendRequestAsync<IPriceService, GetPriceResponse?>((s, ct) => s.GetAsync(UnitType.USD, ct),
            response =>
            {
                _model.UsDollarPrice = response != null ? decimal.Parse(response.Value) / 10m : 0m;
            });

        StateHasChanged();
    }

    private async Task Calculate()
    {
        await _from.Validate();

        if (_from.IsValid)
        {
            _rawPrice = CalculatorHelper.CalculateRawPrice(_model);
            _wage = CalculatorHelper.CalculateWage(_model, _rawPrice.Value);
            _profit = CalculatorHelper.CalculateProfit(_model, _rawPrice.Value, _wage.Value);
            _tax = CalculatorHelper.CalculateTax(_model, _wage.Value, _profit.Value);
            _finalPrice = CalculatorHelper.CalculateFinalPrice(_model, _rawPrice.Value, _wage.Value, _profit.Value, _tax.Value);
        }
        else
        {
            _rawPrice = null;
            _wage = null;
            _profit = null;
            _tax = null;
            _finalPrice = null;
        }
    }

    private async void OnWageTypeChanged(WageType? wageType)
    {
        _model.WageType = wageType;
        switch (wageType)
        {
            case WageType.Percent:
                _wageTypeField.AdornmentIcon = Icons.Material.Filled.Percent;
                _wageField.Disabled = false;
                _wageFieldAdornmentText = "درصد";
                break;
            case WageType.Toman:
                _wageTypeField.AdornmentIcon = Icons.Material.Filled.Money;
                _wageField.Disabled = false;
                _wageFieldAdornmentText = "تومان";
                break;
            case WageType.Dollar:
                _wageTypeField.AdornmentIcon = Icons.Material.Filled.AttachMoney;
                _wageField.Disabled = false;
                _wageFieldAdornmentText = "دلار";
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

    private async void OnDollarPriceChanged(decimal? dollarPrice)
    {
        _model.UsDollarPrice = dollarPrice;

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

    private async void OnAdditionalPricesChanges(decimal? additionalPrices)
    {
        _model.AdditionalPrices = additionalPrices;

        await Calculate();
    }

    private async Task OnBarcodeChanged(string barcode)
    {
        _barcode = barcode;

        if (string.IsNullOrWhiteSpace(barcode))
        {
            OnBarcodeCleared();
            return;
        }

        await SendRequestAsync<IProductService, GetProductResponse?>(async (s, ct) => await s.GetAsync(barcode, ct),
            response =>
            {
                if (response is null)
                    return;

                _model.Weight = response.Weight;
                _model.CaratType = response.CaratType;
                _model.Wage = response.Wage;
                _model.WageType = response.WageType;
                _model.ProductType = response.ProductType;

                OnWageTypeChanged(_model.WageType);
                OnWageChanged(_model.Wage);
                OnProductTypeChanged(_model.ProductType);
            });

        await Calculate();
    }

    private async void OnBarcodeCleared()
    {
        _barcode = null;

        ResetModel();
        await Calculate();
    }

    private void ResetModel()
    {
        _model.Weight = 0;
        _model.CaratType = CaratType.Eighteen;
        _model.Wage = 0;
        _model.WageType = null;
        _model.AdditionalPrices = null;
        _model.ProfitPercent = _settings?.GoldProfitPercent ?? 7;
    }

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
        await LoadPricesAsync();
        StateHasChanged();
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}