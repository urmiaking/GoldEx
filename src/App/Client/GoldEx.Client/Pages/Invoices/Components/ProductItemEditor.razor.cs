using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.Components.Categories;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Customers;
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
    [Parameter] public TradeScale TradeScale { get; set; }
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
    private List<GetCustomerResponse> _customers = [];

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

        if (TradeScale is TradeScale.Wholesale)
        {
            Model.ProfitPercent = 0;
            Model.TaxPercent = 0;
        }

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

    private void Close() => MudDialog.Cancel();

    private void OnProductTypeChanged(ProductType productType)
    {
        Model.Product.ProductType = productType;

        switch (productType)
        {
            case ProductType.Jewelry:
                Model.SetAsJewelry(PriceUnit, _settings);
                break;
            case ProductType.Gold:
                Model.SetAsGold(PriceUnit, _settings);
                break;
            case ProductType.MoltenGold:
                Model.SetAsMoltenGold(_settings);
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

        if (name.Length <= 2)
            return null;

        await SendRequestAsync<IProductService, List<GetProductResponse>>(
            action: (s, ct) => s.GetListAsync(name, Model.Product.ProductType, ct),
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
                Model.Product.WagePriceUnitId = null;
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
                Model.WageExchangeRate = null;
                Model.Product.WagePriceUnitId = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }

        // منطق محاسبه CostPrice اکنون در ProductItemVm.RecalculateAmounts() است
        StateHasChanged();
    }

    private void OnAddGemStone()
    {
        Model.Product.Stones ??= [];
        Model.Product.Stones.Add(new GemStoneVm());
        StateHasChanged();
    }

    private void OnRemoveGemStone(int index)
    {
        Model.Product.Stones?.RemoveAt(index);
        StateHasChanged();
    }

    private void OnProductCategoryChanged(ProductCategoryVm? category)
    {
        Model.Product.CategoryVm = category;
        Model.Product.ProductCategoryId = category?.Id;
        Model.Product.ProductCategoryTitle = category?.Title;
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

    private async Task OnProductStonePriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        Model.Product.StonePriceUnit = priceUnit;

        if (Model.Product.StonePriceUnit != null && PriceUnit.Id != Model.Product.StonePriceUnit.Id)
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(Model.Product.StonePriceUnit.Id, PriceUnit.Id, ct),
                afterSend: response =>
                {
                    if (response.ExchangeRate.HasValue)
                        Model.StonePriceUnitExchangeRate = response.ExchangeRate.Value;
                });

        StateHasChanged();
    }

    private void OnTotalWeightChanged(decimal? totalWeight)
    {
        Model.TotalWeight = totalWeight;
        StateHasChanged();
    }

    private void OnWageChanged(decimal? wage)
    {
        Model.Product.Wage = wage;
        StateHasChanged();
    }

    #region Customer

    private async Task<IEnumerable<CustomerVm>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, CustomerType.AssayingLab, ct),
            afterSend: response =>
            {
                _customers = response;
            },
            cancelPrevious: true);

        return _customers.Select(CustomerVm.CreateFrom);
    }

    private async Task OnAddCustomer()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true },
            { x => x.CustomerType, CustomerType.AssayingLab }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن آزمایشگاه جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            Model.Product.MoltenGold!.Assayer = customerVm;
            StateHasChanged();
        }
    }

    #endregion

    private void OnCostPriceChanged(decimal? costPrice)
    {   
        Model.CostPrice = costPrice;

        if (Model.IsInstantProduct || Model.InvoiceType is InvoiceType.Purchase)
        {
            Model.FinalAmount = Model.CostPrice ?? 0;
        }
    }
}