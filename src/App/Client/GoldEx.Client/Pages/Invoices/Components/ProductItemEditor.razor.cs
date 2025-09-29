using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.Components.Categories;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class ProductItemEditor
{
    [Parameter] public ProductItemVm Model { get; set; } = ProductItemVm.CreateDefaultInstance();
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly ProductItemValidator _productItemValidator = new();

    private MudForm _form = default!;

    private IEnumerable<ProductCategoryVm> _productCategories = [];
    private List<GetProductResponse> _products = [];
    private GetSettingResponse? _settings;

    private bool _weightFieldMenuOpen;
    private bool _wageFieldMenuOpen;
    private bool _costPriceMenuOpen;
    private bool _isProcessing;
    private string? WageFieldAdornmentText => Model.Product.WageType switch
    {
        WageType.Percent => "درصد",
        WageType.Fixed => Model.Product.WagePriceUnitTitle,
        null => null,
        _ => throw new ArgumentOutOfRangeException()
    };

    private string? WageExchangeRateLabel => Model.Product.WageType switch
    {
        WageType.Percent => null,
        WageType.Fixed when PriceUnit.Id != Model.Product.WagePriceUnitId =>
            $"نرخ تبدیل {Model.Product.WagePriceUnitTitle} به {PriceUnit.Title}",
        null => null,
        _ => throw new ArgumentOutOfRangeException()
    };

    private string? StonePriceExchangeRateLabel => Model.Product.StonePriceUnit != null
                                                && PriceUnit.Id != Model.Product.StonePriceUnit.Id
        ? $"نرخ تبدیل {Model.Product.StonePriceUnit.Title} به {PriceUnit.Title}"
        : null;

    public string CostPriceAdornmentText => Model.CostPriceUnitTitle ?? PriceUnit.Title;
    public string? CostExchangeRateLabel => Model.CostPriceUnitId.HasValue && PriceUnit.Id != Model.CostPriceUnitId
        ? $"نرخ تبدیل {Model.CostPriceUnitTitle} به {PriceUnit.Title}"
        : null;

    protected override void OnParametersSet()
    {
        if (!Model.Product.WagePriceUnitId.HasValue && Model.Product.WageType is WageType.Fixed)
        {
            Model.Product.WagePriceUnitId = PriceUnits.FirstOrDefault(x => x.IsDefault)?.Id;
            Model.Product.WagePriceUnitTitle = PriceUnits.FirstOrDefault(x => x.IsDefault)?.Title;
        }

        OnWageTypeChanged(Model.Product.WageType);

        base.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadCategoriesAsync();
        await LoadSettingsAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response => _settings = response);
    }

    private async Task LoadCategoriesAsync()
    {
        await SendRequestAsync<IProductCategoryService, List<GetProductCategoryResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _productCategories = response.Select(ProductCategoryVm.CreateFrom);
            });
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(Model.Product.GoldUnitType, PriceUnit.Id, true, ct),
            afterSend: response =>
            {
                decimal.TryParse(response?.Value, out var gramPriceValue);

                Model.GramPrice = gramPriceValue;

                StateHasChanged();
            });
    }

    private async Task Submit()
    {
        _isProcessing = true;
        await _form.Validate();

        if (!_form.IsValid)
        {
            _isProcessing = false;
            return;
        }

        _isProcessing = false;
        MudDialog.Close(DialogResult.Ok(Model));
    }

    private void GenerateBarcode() => Model.Product.Barcode = StringExtensions.GenerateRandomBarcode();

    private void Close() => MudDialog.Cancel();

    private void OnProductTypeChanged(ProductType productType)
    {
        Model.Product.ProductType = productType;

        switch (productType)
        {
            case ProductType.Jewelry:
                Model.Product.WageType = WageType.Fixed;
                Model.Product.WagePriceUnitId = PriceUnit.Id;
                Model.Product.WagePriceUnitTitle = PriceUnit.Title;
                if (Model.InvoiceType is InvoiceType.Sell)
                {
                    Model.ProfitPercent = _settings?.JewelryProfitPercent ?? 20;
                    Model.TaxPercent = _settings?.TaxPercent ?? 9;
                }
                break;
            case ProductType.Gold:
                Model.Product.WageType = WageType.Percent;
                Model.Product.WagePriceUnitId = null;
                Model.Product.WagePriceUnitTitle = null;
                if (Model.InvoiceType is InvoiceType.Sell)
                {
                    Model.ProfitPercent = _settings?.GoldProfitPercent ?? 7;
                    Model.TaxPercent = _settings?.TaxPercent ?? 9;
                }
                break;
            case ProductType.MoltenGold:
                Model.Product.Wage = null;
                Model.Product.WageType = null;
                Model.Product.WagePriceUnitId = null;
                Model.Product.WagePriceUnitTitle = null;
                Model.ProfitPercent = 0;
                Model.TaxPercent = 0;
                break;
            case ProductType.UsedGold:
                throw new InvalidOperationException();
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
    }

    private void OnProductNameChanged(string name)
    {
        var product = _products.FirstOrDefault(x => x.Name == name);

        if (product != null)
        {
            Model.Product = ProductVm.CreateFromSearch(product);
            OnWageTypeChanged(product.WageType);
        }

        StateHasChanged();
    }

    private async Task<IEnumerable<string>?> SearchNames(string? name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        await SendRequestAsync<IProductService, List<GetProductResponse>>(
            action: (s, ct) => s.GetListAsync(name, ct),
            afterSend: response => _products = response);

        return _products.Select(x => x.Name);
    }

    private async void OnWageTypeChanged(WageType? wageType)
    {
        Model.Product.WageType = wageType;

        switch (wageType)
        {
            case WageType.Percent:
                Model.WageExchangeRate = null;
                break;
            case WageType.Fixed:
                if (Model.Product.WagePriceUnitId.HasValue)
                {
                    await SelectWagePriceUnit(PriceUnits.First(x => x.Id == Model.Product.WagePriceUnitId));
                }
                else
                {
                    await SelectWagePriceUnit(PriceUnits.First(x => x.Id == PriceUnit.Id));
                }
                break;
            case null:
                Model.Product.Wage = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }
    }

    private void OnAddGemStone()
    {
        Model.Product.Stones ??= [];
        Model.Product.Stones.Add(new GemStoneVm
        {
            Code = StringExtensions.GenerateRandomCode(5)
        });
        StateHasChanged();
    }

    private void OnRemoveGemStone(int index)
    {
        Model.Product.Stones?.RemoveAt(index);
        StateHasChanged();
    }

    private void OnProductCategoryChanged(ProductCategoryVm? category)
    {
        if (category is null)
            return;

        Model.Product.CategoryVm = category;
        Model.Product.ProductCategoryId = category.Id;
        Model.Product.ProductCategoryTitle = category.Title;
    }

    private void OnWageAdornmentClicked()
    {
        if (Model.Product.WageType is WageType.Fixed)
            _wageFieldMenuOpen = !_wageFieldMenuOpen;
    }

    private void OnCostPriceAdornmentClicked() => _costPriceMenuOpen = !_costPriceMenuOpen;

    private async Task SelectWagePriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        Model.Product.WagePriceUnitId = priceUnit.Id;
        Model.Product.WagePriceUnitTitle = priceUnit.Title;

        if (PriceUnit.Id != Model.Product.WagePriceUnitId)
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(Model.Product.WagePriceUnitId.Value, PriceUnit.Id, ct),
                afterSend: response =>
                {
                    if (response.ExchangeRate.HasValue)
                        Model.WageExchangeRate = response.ExchangeRate.Value;
                });

        StateHasChanged();
    }

    private async Task SelectCostPriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        Model.CostPriceUnitId = priceUnit.Id;
        Model.CostPriceUnitTitle = priceUnit.Title;
        Model.CostPriceExchangeRate = null;

        if (PriceUnit.Id != Model.CostPriceUnitId)
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(Model.CostPriceUnitId.Value, PriceUnit.Id, ct),
                afterSend: response =>
                {
                    if (response.ExchangeRate.HasValue)
                        Model.CostPriceExchangeRate = response.ExchangeRate.Value;
                });

        StateHasChanged();
    }

    private async Task OnAddCategory()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var dialog = await DialogService.ShowAsync<Editor>("افزودن دسته بندی جدید", dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: true })
        {
            await LoadCategoriesAsync();
            StateHasChanged();
        }
    }

    private async Task OnGoldUnitTypeSelected(GoldUnitType unitType)
    {
        Model.Product.GoldUnitType = unitType;
        await LoadGramPriceAsync();
    }
	private async Task OnProductStonePriceUnitChanged(GetPriceUnitTitleResponse priceUnit)
	{
        Model.Product.StonePriceUnit = priceUnit;

        if (PriceUnit.Id != Model.Product.StonePriceUnit.Id)
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(Model.Product.StonePriceUnit.Id, PriceUnit.Id, ct),
                afterSend: response =>
                {
                    if (response.ExchangeRate.HasValue)
                        Model.StonePriceUnitExchangeRate = response.ExchangeRate.Value;
                });

        StateHasChanged();
    }
}