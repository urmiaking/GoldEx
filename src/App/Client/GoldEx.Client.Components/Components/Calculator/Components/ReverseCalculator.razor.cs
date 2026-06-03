using GoldEx.Client.Components.Calculator.Validators;
using GoldEx.Client.Components.Calculator.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Components.Calculator.Components;

public partial class ReverseCalculator
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;

    private readonly ReverseCalculatorVm _model = new();
    private MudForm _form = default!;
    private readonly ReverseCalculatorValidator _calculatorValidator = new();
    private Timer? _timer;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(30);
    private List<GetPriceUnitTitleResponse> _priceUnits = [];

    private decimal? _finalValue;
    private const bool ApplySafetyMargin = true;

    private string PriceUnitAdornment => DefaultPriceUnit.Title;

    private GetPriceUnitTitleResponse DefaultPriceUnit => _priceUnits.FirstOrDefault(x => x.IsDefault)
                                                          ?? new GetPriceUnitTitleResponse(Guid.Empty, "تومان", false, true, false);

    #region InitialLoad

    protected override async Task OnInitializedAsync()
    {
        try
        {
            await LoadPriceUnitsAsync();
            await StartTimer();
        }
        finally
        {
            StateHasChanged();
        }
        await base.OnInitializedAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
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
            });
    }

    private async Task LoadUnitPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(_model.GoldUnitType, _model.PriceUnit?.Id, ApplySafetyMargin, ct),
            afterSend: response =>
            {
                decimal.TryParse(response?.Value, out var unitPriceValue);

                _model.UnitPrice = unitPriceValue;

                StateHasChanged();
            },
            createScope: true);
    }

    #endregion

    #region Events

    private async Task OnPriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        _model.PriceUnit = priceUnit;

        await LoadUnitPriceAsync();

        await Calculate();
    }

    private async Task Calculate()
    {
        try
        {
            if (_model.Price is null || _model.Fineness is null)
            {
                _finalValue = null;
                return;
            }

            await _form.ValidateAsync();

            if (_form.IsValid)
            {
                _finalValue = CalculatorHelper.MoltenGold.CalculateWeight(_model.Price.Value, _model.Fineness.Value, _model.UnitPrice, _model.ProfitPercent);
            }
            else
            {
                _finalValue = null;
            }
        }
        finally
        {
            StateHasChanged();
        }
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

            await LoadUnitPriceAsync();
            await Calculate();
        });
    }

    public override async ValueTask DisposeAsync()
    {
        if (_timer is not null) await _timer.DisposeAsync();

        await base.DisposeAsync();
    }

    #endregion

    private async Task OnGoldUnitTypeChanged(GoldUnitType unitType)
    {
        _model.GoldUnitType = unitType;
        await LoadUnitPriceAsync();
        await Calculate();
    }

    private Task OnUnitPriceChanged(decimal unitPrice)
    {
        _model.UnitPrice = unitPrice;
        return Calculate();
    }


    private async Task OnPriceChanged(decimal? price)
    {
        _model.Price = price;
        await Calculate();
    }

    private async Task OnFinenessChanged(decimal? fineness)
    {
        _model.Fineness = fineness;
        await Calculate();
    }

    private async Task OnProfitChanged(decimal? profitPercent)
    {
        _model.ProfitPercent = profitPercent;
        await Calculate();
    }
}