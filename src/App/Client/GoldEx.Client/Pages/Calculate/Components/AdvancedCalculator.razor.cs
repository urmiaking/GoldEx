using GoldEx.Client.Pages.Calculate.ViewModels;
using GoldEx.Sdk.Common.Data;
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
using MudBlazor;

namespace GoldEx.Client.Pages.Calculate.Components;

public partial class AdvancedCalculator
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;

    private Timer? _timer;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(30);

    private GetSettingResponse? _settings;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private AdvancedCalculatorVm _model = new();
    private MudTable<GetInventoryStockResponse> _table = default!;

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
            },
            createScope: true);
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
            },
            createScope: true);
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
            },
            createScope: true);
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

    private async Task<TableData<GetInventoryStockResponse>> LoadProductsAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<GetInventoryStockResponse>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, null, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        var calculatorFilter = _model.ToFilterRequest();

        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (s, ct) => s.GetAvailableProductsAsync(calculatorFilter, filter, ct),
            afterSend: async response =>
            {
                result.Items = response.Data;
                result.TotalItems = response.Total;

                foreach (var item in response.Data)
                {
                    item.FinalPrice = await CalculateFinalPriceAsync(item, _model.PriceUnit);
                }
            },
            createScope: true);
        return result;
    }

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task OnSearch()
    {
        await RefreshAsync();

        StateHasChanged();
    }

    private async Task<decimal> CalculateFinalPriceAsync(GetInventoryStockResponse? item, GetPriceUnitTitleResponse? contextPriceUnit)
    {
        if (item?.Product is null)
            return 0;

        var rawPrice = CalculatorHelper.Product.CalculateRawPrice(
            item.CurrentAmount,
            _model.GramPrice,
            item.Product.Fineness,
            1,
            item.Product.ProductType);

        var wageAmount = CalculatorHelper.Product.CalculateWage(rawPrice, item.CurrentAmount, item.Product.Wage, item.Product.WageType, null);
        var profitAmount = CalculatorHelper.Product.CalculateProfit(rawPrice, wageAmount, item.Product.ProductType, _model.ProfitPercent);

        decimal stoneAmount = 0;
        if (item.Product.GemStones is not null && item.Product.GemStones.Count > 0)
        {
            foreach (var stone in item.Product.GemStones)
            {
                var stonePrice = stone.Cost;

                if (item.Product.StonePriceUnit != null && contextPriceUnit != null &&
                    item.Product.StonePriceUnit.Id != contextPriceUnit.Id)
                {
                    await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                        action: (s, ct) => s.GetExchangeRateAsync(item.Product.StonePriceUnit.Id, contextPriceUnit.Id, ct),
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
            item.Product.ProductType,
            stoneAmount);

        var finalPrice = CalculatorHelper.Product.CalculateFinalPrice(
            rawPrice,
            wageAmount,
            profitAmount,
            taxAmount,
            stoneAmount,
            item.Product.ProductType);

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
        Navigation.NavigateTo(ClientRoutes.Invoices.Create.AppendQueryString(new { barcode = product?.Barcode })
            .AppendQueryString(new { TradeScale = TradeScale.Retail }));
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

            await RefreshPricesAsync();

            StateHasChanged();
        });
    }

    private async Task RefreshPricesAsync()
    {
        foreach (var item in _table.FilteredItems)
        {
            item.FinalPrice = await CalculateFinalPriceAsync(item, _model.PriceUnit);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_timer is not null) 
            await _timer.DisposeAsync();

        await base.DisposeAsync();
    }

    #endregion

    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);
    }

    private async Task OnGramPriceChanged(decimal gramPrice)
    {
        _model.GramPrice = gramPrice;
        await RefreshPricesAsync();

        StateHasChanged();
    }
}