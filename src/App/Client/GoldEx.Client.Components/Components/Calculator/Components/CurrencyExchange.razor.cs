using GoldEx.Client.Components.Calculator.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Components.Calculator.Components;

public partial class CurrencyExchange
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public int Elevation { get; set; } = 24;

    private readonly CurrencyExchangeVm _model = new();
    private List<GetPriceUnitTitleResponse> _priceUnits = [];

    private bool _feeFieldMenuOpen;

    private const decimal MinReasonableDisplayedRate = 0.0001m;
    private const string PriceUnitsKey = "PriceUnits";
    private const string ExchangeRateKey = "ExchangeRate";

    private void SetExchangeRateFromEffective(decimal? effectiveRate)
    {
        if (effectiveRate is null)
        {
            _model.ExchangeRate = null;
            _model.IsExchangeRateInverted = false;
            return;
        }

        if (effectiveRate <= 0)
        {
            _model.ExchangeRate = effectiveRate;
            _model.IsExchangeRateInverted = false;
            return;
        }

        if (effectiveRate < MinReasonableDisplayedRate)
        {
            _model.ExchangeRate = 1m / effectiveRate.Value;
            _model.IsExchangeRateInverted = true;
            return;
        }

        _model.ExchangeRate = effectiveRate;
        _model.IsExchangeRateInverted = false;
    }

    private decimal? GetEffectiveRate() => _model.EffectiveExchangeRate;

    private string? FeeFieldAdornmentText => _model.FeeType switch
    {
        WageType.Percent => WageType.Percent.GetDisplayName(),
        WageType.Fixed => _model.FeePriceUnit?.Title,
        null => null,
        _ => throw new ArgumentOutOfRangeException(nameof(_model.FeeType), _model.FeeType, null)
    };

    private string? FeeTypeAdornmentIcon =>
        _model.FeeType switch
        {
            WageType.Percent => Icons.Material.Filled.Percent,
            WageType.Fixed => Icons.Material.Filled.Money,
            null => Icons.Material.Filled.MoneyOff,
            _ => throw new ArgumentOutOfRangeException(nameof(_model.FeeType), _model.FeeType, null)
        };

    private string? FeeExchangeRateLabel =>
        _model is { FeeType: WageType.Fixed, FeePriceUnit: not null, DestinationPriceUnit: not null }
            ? $"نرخ تبدیل {_model.FeePriceUnit.Title} به {_model.DestinationPriceUnit.Title}"
            : null;

    protected override async Task OnInitializedAsync()
    {
        RestorePersistedState();
        await LoadPriceUnitsAsync();
        await EnsureExchangeRateLoadedAsync(force: true);
        await RecalculateAsync();

        await base.OnInitializedAsync();
    }

    protected override Task OnPersisting()
    {
        PersistStateAsJson(PriceUnitsKey, _priceUnits);
        PersistStateAsJson(ExchangeRateKey, _model.ExchangeRate);

        return base.OnPersisting();
    }

    private void RestorePersistedState()
    {
        if (RestoreStateFromJson(PriceUnitsKey, out List<GetPriceUnitTitleResponse>? persistedUnits) && persistedUnits is not null)
        {
            _priceUnits = persistedUnits;
        }

        if (RestoreStateFromJson(ExchangeRateKey, out decimal? persistedRate) && persistedRate is not null)
        {
            SetExchangeRateFromEffective(persistedRate);
        }
    }

    private async Task LoadPriceUnitsAsync()
    {
        if (_priceUnits.Count > 0)
        {
            EnsureDefaultUnitsSelected();
            return;
        }

        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response;

                EnsureDefaultUnitsSelected();
            });
    }

    private void EnsureDefaultUnitsSelected()
    {
        var defaultUnit = _priceUnits.FirstOrDefault(x => x.IsDefault);
        var dollarUnit = _priceUnits.FirstOrDefault(x => x.Title.Contains("دلار"));

        _model.SourcePriceUnit ??= dollarUnit;
        _model.DestinationPriceUnit ??= defaultUnit;
        _model.FeePriceUnit ??= _model.DestinationPriceUnit ?? defaultUnit;
    }

    private async Task OnFeeTypeChanged(WageType? feeType)
    {
        _model.FeeType = feeType;

        switch (feeType)
        {
            case WageType.Percent:
                _model.FeeExchangeRate = null;
                _model.FeePriceUnit ??= _model.DestinationPriceUnit;
                break;

            case WageType.Fixed:
                _model.FeePriceUnit ??= _model.DestinationPriceUnit;
                _model.FeeExchangeRate = null;
                if (_model.FeePriceUnit is not null)
                    await SelectFeePriceUnit(_model.FeePriceUnit);
                break;

            case null:
                _model.Fee = null;
                _model.FeeExchangeRate = null;
                _model.FeeInDestination = null;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(feeType), feeType, null);
        }

        await RecalculateAsync();
    }

    private async Task OnFeeChanged(decimal? fee)
    {
        _model.Fee = fee;
        await RecalculateAsync();
    }

    private async Task OnFeeExchangeRateChanged(decimal? exchangeRate)
    {
        _model.FeeExchangeRate = exchangeRate;
        await RecalculateAsync();
    }

    private async Task SelectFeePriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        _model.FeePriceUnit = priceUnit;
        _model.FeeExchangeRate = null;

        if (_model.DestinationPriceUnit is null)
            return;

        if (_model.FeePriceUnit.Id == _model.DestinationPriceUnit.Id)
        {
            _model.FeeExchangeRate = 1;
            return;
        }

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(_model.FeePriceUnit.Id, _model.DestinationPriceUnit.Id, ct),
            afterSend: response =>
            {
                if (response.ExchangeRate.HasValue)
                    _model.FeeExchangeRate = response.ExchangeRate.Value;

                StateHasChanged();
            },
            cancelPrevious: true);
    }

    private async Task OnSourcePriceUnitChanged(GetPriceUnitTitleResponse? unit)
    {
        _model.SourcePriceUnit = unit;
        _model.ExchangeRate = null;
        _model.IsExchangeRateInverted = false;
        await EnsureExchangeRateLoadedAsync(force: true);
        await RecalculateAsync();
    }

    private async Task OnDestinationPriceUnitChanged(GetPriceUnitTitleResponse? unit)
    {
        _model.DestinationPriceUnit = unit;
        _model.ExchangeRate = null;
        _model.IsExchangeRateInverted = false;
        await EnsureExchangeRateLoadedAsync(force: true);
        await RecalculateAsync();
    }

    private async Task OnSourceAmountChanged(decimal? amount)
    {
        _model.SourceAmount = amount;
        await RecalculateAsync();
    }

    private async Task OnExchangeRateChanged(decimal? exchangeRate)
    {
        _model.ExchangeRate = exchangeRate;
        await RecalculateAsync();
    }

    private async Task EnsureExchangeRateLoadedAsync(bool force)
    {
        if (_model.SourcePriceUnit is null || _model.DestinationPriceUnit is null)
            return;

        if (_model.SourcePriceUnit.Id == _model.DestinationPriceUnit.Id)
        {
            _model.ExchangeRate = 1;
            _model.IsExchangeRateInverted = false;
            return;
        }

        var effectiveRate = GetEffectiveRate();
        if (!force && effectiveRate is > 0)
            return;

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(_model.SourcePriceUnit.Id, _model.DestinationPriceUnit.Id, ct),
            afterSend: response =>
            {
                SetExchangeRateFromEffective(response.ExchangeRate);
            },
            cancelPrevious: true);
    }

    private async Task RecalculateAsync()
    {
        if (_model.SourcePriceUnit is null || _model.DestinationPriceUnit is null || !_model.SourceAmount.HasValue)
        {
            _model.DestinationAmount = null;
            _model.FeeInDestination = null;
            _model.FinalDestinationAmount = null;
            StateHasChanged();
            return;
        }

        if (_model.SourceAmount <= 0)
        {
            _model.DestinationAmount = 0;
            _model.ExchangeRate = _model.SourcePriceUnit.Id == _model.DestinationPriceUnit.Id ? 1 : null;
            _model.IsExchangeRateInverted = false;
            _model.FeeInDestination = 0;
            _model.FinalDestinationAmount = 0;
            StateHasChanged();
            return;
        }

        if (_model.SourcePriceUnit.Id == _model.DestinationPriceUnit.Id)
        {
            _model.ExchangeRate = 1;
            _model.IsExchangeRateInverted = false;
            _model.DestinationAmount = _model.SourceAmount;
            CalculateFeeAndFinalize();
            StateHasChanged();
            return;
        }

        if (GetEffectiveRate() is null)
            await EnsureExchangeRateLoadedAsync(force: false);

        var effectiveRate = GetEffectiveRate();
        if (effectiveRate is > 0)
        {
            _model.DestinationAmount = _model.SourceAmount.Value * effectiveRate.Value;
            CalculateFeeAndFinalize();
            StateHasChanged();
            return;
        }

        StateHasChanged();

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(
                _model.SourcePriceUnit.Id,
                _model.DestinationPriceUnit.Id,
                ct),
            afterSend: response =>
            {
                var rate = response.ExchangeRate;
                SetExchangeRateFromEffective(rate);

                var r = GetEffectiveRate();
                _model.DestinationAmount = r.HasValue
                    ? _model.SourceAmount.Value * r.Value
                    : null;

                CalculateFeeAndFinalize();

                StateHasChanged();
            },
            cancelPrevious: true);

        StateHasChanged();
    }

    private void CalculateFeeAndFinalize()
    {
        if (!_model.DestinationAmount.HasValue)
        {
            _model.FeeInDestination = null;
            _model.FinalDestinationAmount = null;
            return;
        }

        var feeInDestination = 0m;

        if (_model.FeeType is WageType.Percent)
        {
            if (_model.Fee is > 0)
                feeInDestination = _model.DestinationAmount.Value * (_model.Fee.Value / 100m);
        }
        else if (_model.FeeType is WageType.Fixed)
        {
            if (_model.Fee is > 0)
            {
                var rate = _model.FeeExchangeRate;
                if (_model.FeePriceUnit is not null && _model.DestinationPriceUnit is not null && _model.FeePriceUnit.Id == _model.DestinationPriceUnit.Id)
                    rate = 1;

                if (rate.HasValue)
                    feeInDestination = _model.Fee.Value * rate.Value;
                else
                    feeInDestination = 0m;
            }
        }

        _model.FeeInDestination = feeInDestination;
        _model.FinalDestinationAmount = _model.DestinationAmount.Value + feeInDestination;
    }
}