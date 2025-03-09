using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Calculate.Validators;
using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class Calculator
{
    private CalculatorVm _model = new();
    private MudForm _from = default!;
    private CalculatorValidator _calculatorValidator = new();
    private MudSelect<WageType?> _wageTypeField = default!;
    private MudTextField<double?> _wageField = default!;
    private MudTextField<double> _profitField = default!;

    private string? _wageFieldAdornmentText = "درصد";
    private string? _gramPriceFieldHelperText;
    private string? _usDollarPriceFieldHelperText;
    private string? _wageFieldHelperText;
    private double? _rawPrice;
    private double? _wage;
    private double? _profit;
    private double? _tax;
    private double? _finalPrice;
    private string? _additionalPricesFieldHelperText;

    private IPriceClientService PriceService => GetRequiredService<IPriceClientService>();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await LoadPricesAsync();
        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task LoadPricesAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var gram18Price = await PriceService.GetGram18PriceAsync(CancellationTokenSource.Token);
            var usDollarPrice = await PriceService.GetUsDollarPriceAsync(CancellationTokenSource.Token);

            if (gram18Price is not null)
            {
                _model.GramPrice = double.Parse(gram18Price.Value) / 10;
                _gramPriceFieldHelperText = $"{_model.GramPrice:N0} تومان";
            }

            if (usDollarPrice is not null)
            {
                _model.UsDollarPrice = double.Parse(usDollarPrice.Value) / 10;
                _usDollarPriceFieldHelperText = $"{_model.UsDollarPrice:N0} تومان";
            }

            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            SetIdeal();
        }
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
                _wageFieldHelperText = null;
                break;
            case WageType.Rial:
                _wageTypeField.AdornmentIcon = Icons.Material.Filled.Money;
                _wageField.Disabled = false;
                _wageFieldAdornmentText = "تومان";
                _wageFieldHelperText = _model.Wage.HasValue ? $"{_model.Wage:N0} تومان" : null;
                break;
            case WageType.Dollar:
                _wageTypeField.AdornmentIcon = Icons.Material.Filled.AttachMoney;
                _wageField.Disabled = false;
                _wageFieldAdornmentText = "دلار";
                _wageFieldHelperText = _model.Wage.HasValue ? $"{_model.Wage:N0} دلار" : null;
                break;
            case null:
                await _wageField.Clear();
                _wageFieldAdornmentText = null;
                _wageField.Disabled = true;
                _wageFieldHelperText = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }

        await Calculate();
    }

    private async void OnWageChanged(double? wage)
    {
        _model.Wage = wage;

        _wageFieldHelperText = _model.WageType switch
        {
            WageType.Rial => $"{wage:N0} تومان",
            WageType.Dollar => $"{wage:N0} دلار",
            _ => _wageFieldHelperText
        };

        await Calculate();
    }

    private async void OnProductTypeChanged(ProductType productType)
    {
        _model.ProductType = productType;
        switch (productType)
        {
            case ProductType.Jewelry:
                break;
            case ProductType.Gold:
                break;
            case ProductType.Coin:
                break;
            case ProductType.MoltenGold:
                break;
            case ProductType.UsedGold:
                await _wageField.Clear();
                await _wageTypeField.ResetAsync();
                await _profitField.Clear();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }

        await Calculate();
    }

    private async void OnDollarPriceChanged(double? dollarPrice)
    {
        _model.UsDollarPrice = dollarPrice;
        _usDollarPriceFieldHelperText = $"{dollarPrice:N0} تومان";

        await Calculate();
    }

    private async void OnCaratTypeChanged(CaratType caratType)
    {
        _model.CaratType = caratType;

        await Calculate();
    }

    private async void OnGramPriceChanged(double gramPrice)
    {
        _model.GramPrice = gramPrice;

        _gramPriceFieldHelperText = $"{gramPrice:N0} تومان";

        await Calculate();
    }

    private async void OnWeightChanged(double weight)
    {
        _model.Weight = weight;

        await Calculate();
    }

    private async void OnProfitChanged(double profit)
    {
        _model.Profit = profit;

        await Calculate();
    }

    private async void OnAdditionalPricesChanges(double? additionalPrices)
    {
        _model.AdditionalPrices = additionalPrices;
        _additionalPricesFieldHelperText = additionalPrices.HasValue ? $"{additionalPrices:N0} تومان" : null;

        await Calculate();
    }
}