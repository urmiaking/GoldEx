using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class ReverseCalculator
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;

    private Timer? _timer;
    private TimeSpan _updateInterval = TimeSpan.FromSeconds(30);

    private GetSettingResponse? _settings;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private ReverseCalculatorVm _model = new();

    private bool _isSearching;

    private List<GetInventoryStockResponse> _results = [];
    private List<GetProductCategoryResponse> _productCategories = [];
    private bool _minWeightFieldMenuOpen;
    private bool _maxWeightFieldMenuOpen;

    protected override async Task OnParametersSetAsync()
    {
        try
        {
            await LoadPriceUnitsAsync();
            await LoadSettingsAsync();
            await LoadCategoriesAsync();
            await StartTimer();
        }
        finally
        {
            StateHasChanged();
        }
        await base.OnParametersSetAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        await SendRequestAsync<IProductCategoryService, List<GetProductCategoryResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response => _productCategories = response);
    }

    #region Load Initial Data

    private async Task LoadPriceUnitsAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authenticationState.User.Identity is { IsAuthenticated: false })
        {
            var priceUnit = new GetPriceUnitTitleResponse(Guid.Empty, "ریال", false, true);

            _model.PriceUnit = priceUnit;

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

                _model.ProfitPercent = _settings?.GoldProfitPercent ?? 7;
                _model.TaxPercent = _settings?.TaxPercent ?? 10;
            });
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(_model.UnitType, _model.PriceUnit?.Id, true, ct),
            afterSend: response =>
            {
                decimal.TryParse(response?.Value, out var gramPriceValue);

                _model.GramPrice = gramPriceValue;

                StateHasChanged();
            });
    }

    #endregion

    #region OnChange

    private async Task OnPriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        _model.PriceUnit = priceUnit;

        await LoadGramPriceAsync();

        StateHasChanged();
    }

    private void OnProductTypeChanged(ProductType productType)
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
                _model.ProfitPercent = _settings?.MoltenGoldCommissionPercent ?? 1.5m;
                break;
            case ProductType.UsedGold:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
    }

    #endregion

    private async Task ResetModel()
    {
        _model.SetNull();
        _results.Clear();

        await LoadGramPriceAsync();
    }

    private async Task OnSearch()
    {
        _isSearching = true;
        _results.Clear();

        await SendRequestAsync<IInventoryStockService, List<GetInventoryStockResponse>>(
            action: (s, ct) => s.GetAvailableProductsAsync(_model.ToFilterRequest(), ct),
            afterSend: response =>
            {
                _results = response;
            });

        _isSearching = false;
        StateHasChanged();
    }

    private decimal CalculateFinalPrice(GetProductResponse? product)
    {
        if (product is null)
            return 0;

        var rawPrice = product.Weight * _model.GramPrice;

        var wageAmount = 0m;
        if (product.WageType == WageType.Percent)
        {
            wageAmount = rawPrice * (product.Wage!.Value / 100);
        }

        var profitAmount = (rawPrice + wageAmount) * (_model.ProfitPercent / 100);

        var taxAmount = (wageAmount + profitAmount) * (_model.TaxPercent / 100);

        var finalPrice = rawPrice + wageAmount + profitAmount + taxAmount;

        return finalPrice;
    }

    private void OnProductCategoryCleared()
    {
        _model.ProductCategory = null;
    }

    private async Task SelectUnitType(GoldUnitType item)
    {
        _model.UnitType = item;
        await LoadGramPriceAsync();

        StateHasChanged();
    }

    private void OnSellProduct(GetProductResponse? product)
    {
        Navigation.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = "" }).AppendQueryString(new { barcode = product?.Barcode }));
    }

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
            StateHasChanged();
        });
    }

    public override async ValueTask DisposeAsync()
    {
        if (_timer is not null) 
            await _timer.DisposeAsync();

        await base.DisposeAsync();
    }

    #endregion
}