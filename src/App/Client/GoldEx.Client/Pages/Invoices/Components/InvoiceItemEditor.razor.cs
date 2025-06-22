using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.Components.Categories;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class InvoiceItemEditor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public InvoiceItemVm Model { get; set; } = InvoiceItemVm.CreateDefaultInstance();
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly InvoiceItemValidator _invoiceItemValidator = new();
    private MudForm _form = default!;
    private MudNumericField<decimal?> _wageField = new();
    private MudSelect<WageType?> _wageTypeField = default!;
    private IEnumerable<ProductCategoryVm> _productCategories = [];
    private bool _wageFieldMenuOpen;
    private string? _wageFieldAdornmentText = "درصد";
    private string? _wageExchangeRateLabel;

    protected override void OnParametersSet()
    {
        if (Id is null)
            GenerateBarcode();

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

        await base.OnParametersSetAsync();
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

    private async Task Submit()
    {
        await _form.Validate();

        if (!_form.IsValid)
            return;

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
                break;
            case ProductType.Gold:
                break;
            case ProductType.MoltenGold:
                Model.Product.Wage = null;
                Model.Product.WageType = null;
                break;
            case ProductType.OldGold:
                Model.Product.Wage = null;
                Model.Product.WageType = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
    }

    private async void OnWageTypeChanged(WageType? wageType)
    {
        Model.Product.WageType = wageType;

        switch (wageType)
        {
            case WageType.Percent:
                UpdateWageFields();
                //_wageTypeField.AdornmentIcon = Icons.Material.Filled.Percent;
                //_wageField.Disabled = false;
                break;
            case WageType.Fixed:
                UpdateWageFields();

                if (Model.Product.WagePriceUnitId.HasValue)
                    await SelectWagePriceUnit(PriceUnits.First(x => x.Id == Model.Product.WagePriceUnitId));

                //_wageTypeField.AdornmentIcon = Icons.Material.Filled.Money;
                //_wageField.Disabled = false;
                break;
            case null:
                await _wageField.ResetAsync();
                //_wageFieldAdornmentText = null;
                //_wageField.Disabled = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }

        OnWageChanged(Model.Product.Wage);
    }

    private void OnAddGemStone(MouseEventArgs obj)
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

    private void OnWageChanged(decimal? wage)
    {
        Model.Product.Wage = wage;
    }

    private void OnWageAdornmentClicked()
    {
        if (Model.Product.WageType is WageType.Fixed) 
            _wageFieldMenuOpen = !_wageFieldMenuOpen;
    }

    private async Task SelectWagePriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        Model.Product.WagePriceUnitId = priceUnit.Id;
        Model.Product.WagePriceUnitTitle = priceUnit.Title;

        if (Model.PriceUnit is null)
            return;

        if (Model.PriceUnit.Id != Model.Product.WagePriceUnitId)
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(Model.Product.WagePriceUnitId.Value, Model.PriceUnit.Id, ct),
                afterSend: response =>
                {
                    if (response.ExchangeRate.HasValue)
                    {
                        Model.ExchangeRate = response.ExchangeRate.Value;
                    }
                });

        UpdateWageFields();
        StateHasChanged();
    }

    private void UpdateWageFields()
    {
        if (Model.Product.WageType is WageType.Fixed)
        {
            _wageFieldAdornmentText = Model.Product.WagePriceUnitTitle;

            if (Model.Product.WagePriceUnitId != Model.PriceUnit?.Id)
            {
                _wageExchangeRateLabel = $"نرخ تبدیل {Model.Product.WagePriceUnitTitle} به {Model.PriceUnit?.Title}";
            }
        }
        else
        {
            _wageFieldAdornmentText = "درصد";
            _wageExchangeRateLabel = null;
            Model.ExchangeRate = null;
        }
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
}