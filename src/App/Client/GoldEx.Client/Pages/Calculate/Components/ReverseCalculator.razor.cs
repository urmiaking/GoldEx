using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
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
            var priceUnit = new GetPriceUnitTitleResponse(Guid.Empty, "ریال", false, true, false);

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

    private async Task OnProductTypeChanged(ProductType productType)
    {
        _model.ProductType = productType;

        _model.ProfitPercent = productType switch
        {
            ProductType.Jewelry => _settings?.JewelryProfitPercent ?? 20,
            ProductType.Gold => _settings?.GoldProfitPercent ?? 7,
            ProductType.MoltenGold => _settings?.MoltenGoldCommissionPercent ?? 1.5m,
            ProductType.UsedGold => throw new ArgumentOutOfRangeException(nameof(productType), productType, null),
            _ => throw new ArgumentOutOfRangeException(nameof(productType), productType, null)
        };

        await OnSearch();
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
            afterSend: async response =>
            {
                _results = response;

                foreach (var item in _results)
                {
                    item.FinalPrice = await CalculateFinalPriceAsync(item.Product, _model.PriceUnit);
                }
            });

        _isSearching = false;
        StateHasChanged();
    }

    private async Task<decimal> CalculateFinalPriceAsync(GetProductResponse? product, GetPriceUnitTitleResponse? contextPriceUnit)
    {
        if (product is null)
            return 0;

        var rawPrice = CalculatorHelper.Product.CalculateRawPrice(
            product.Weight,
            _model.GramPrice,
            product.Fineness,
            1,
            product.ProductType);

        var wageAmount = CalculatorHelper.Product.CalculateWage(rawPrice, product.Weight, product.Wage, product.WageType, null);
        var profitAmount = CalculatorHelper.Product.CalculateProfit(rawPrice, wageAmount, product.ProductType, _model.ProfitPercent);

        decimal stoneAmount = 0;
        if (product.GemStones is not null && product.GemStones.Count > 0)
        {
            foreach (var stone in product.GemStones)
            {
                var stonePrice = stone.Cost;

                if (product.StonePriceUnit != null && contextPriceUnit != null &&
                    product.StonePriceUnit.Id != contextPriceUnit.Id)
                {
                    await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                        action: (s, ct) => s.GetExchangeRateAsync(product.StonePriceUnit.Id, contextPriceUnit.Id, ct),
                        afterSend: response =>
                        {
                            if (response.ExchangeRate.HasValue)
                            {
                                var exchangeRate = response.ExchangeRate.Value;
                                stonePrice *= exchangeRate;
                            }
                        });
                }

                stoneAmount += stonePrice;
            }
        }

        var taxAmount = CalculatorHelper.Product.CalculateTax(
            wageAmount,
            profitAmount,
            _model.TaxPercent,
            product.ProductType,
            stoneAmount);

        var finalPrice = CalculatorHelper.Product.CalculateFinalPrice(
            rawPrice,
            wageAmount,
            profitAmount,
            taxAmount,
            stoneAmount,
            product.ProductType);

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
        Navigation.NavigateTo(ClientRoutes.Invoices.Create.AppendQueryString(new { barcode = product?.Barcode }));
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