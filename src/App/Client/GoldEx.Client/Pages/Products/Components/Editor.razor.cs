using GoldEx.Client.Pages.Products.Validators;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class Editor
{
    [Parameter] public ProductVm Model { get; set; } = ProductVm.CreateDefaultInstance();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private MudForm _form = default!;
    private readonly ProductValidator _productValidator = new();

    private IEnumerable<ProductCategoryVm> _productCategories = [];
    private List<GetProductResponse> _products = [];
    private List<GetPriceUnitTitleResponse> _priceUnits = [];

    private bool _processing;

    private string? WageFieldAdornmentText => Model.WageType switch
    {
        WageType.Percent => "درصد",
        WageType.Fixed => Model.WagePriceUnitTitle,
        null => null,
        _ => throw new ArgumentOutOfRangeException()
    };

    protected override void OnParametersSet()
    {
        OnWageTypeChanged(Model.WageType);

        base.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadCategoriesAsync();
        await LoadPricesAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadPricesAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response;

                //var defaultUnit = _priceUnits.FirstOrDefault(x => x.IsDefault);

                //if (Model.ProductType is ProductType.Jewelry)
                //{
                //    Model.WagePriceUnitId ??= defaultUnit?.Id;
                //    Model.WagePriceUnitTitle ??= defaultUnit?.Title;
                //}
            });
    }

    private async Task LoadCategoriesAsync()
    {
        await SendRequestAsync<IProductCategoryService, List<GetProductCategoryResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response => _productCategories = response.Select(ProductCategoryVm.CreateFrom));
    }

    private async Task Submit()
    {
        await _form.Validate();

        if (!_form.IsValid)
            return;

        _processing = true;

        var request = ProductVm.ToRequest(Model);

        if (!Model.Id.HasValue)
        {
            return;
            //await SendRequestAsync<IProductService>(
            //    action: (s, ct) => s.CreateAsync(request, ct),
            //    afterSend: () =>
            //    {
            //        MudDialog.Close(DialogResult.Ok(true));
            //        return Task.CompletedTask;
            //    });
        }

        await SendRequestAsync<IProductService>(
            action: (s, ct) => s.UpdateAsync(Model.Id.Value, request, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });

        _processing = false;
    }

    private void Close() => MudDialog.Cancel();

    private void OnProductTypeChanged(ProductType productType)
    {
        Model.ProductType = productType;

        switch (productType)
        {
            case ProductType.Jewelry:
                OnWageTypeChanged(WageType.Fixed);
                break;
            case ProductType.Gold:
                OnWageTypeChanged(WageType.Percent);
                break;
            case ProductType.MoltenGold:
                OnWageTypeChanged(null);
                break;
            case ProductType.UsedGold:
                throw new InvalidOperationException();
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
    }

    private void OnWageTypeChanged(WageType? wageType)
    {
        Model.WageType = wageType;

        if (wageType is null)
            Model.Wage = null;

        if (wageType is WageType.Fixed)
        {
            var defaultUnit = _priceUnits.FirstOrDefault(x => x.IsDefault);

            Model.WagePriceUnitId ??= defaultUnit?.Id;
            Model.WagePriceUnitTitle ??= defaultUnit?.Title;
        }
        else
        {
            Model.WagePriceUnitId = null;
            Model.WagePriceUnitTitle = null;
        }
    }

    private void OnAddGemStone()
    {
        Model.Stones ??= [];
        Model.Stones.Add(new GemStoneVm());
        StateHasChanged();
    }

    private void OnRemoveGemStone(int index)
    {
        Model.Stones?.RemoveAt(index);
        StateHasChanged();
    }

    private void OnProductCategoryChanged(ProductCategoryVm? category)
    {
        Model.CategoryVm = category;
        Model.ProductCategoryId = category?.Id;
        Model.ProductCategoryTitle = category?.Title;
    }

    private void SelectWagePriceUnit(GetPriceUnitTitleResponse item)
    {
        Model.WagePriceUnitId = item.Id;
        Model.WagePriceUnitTitle = item.Title;

        OnWageTypeChanged(Model.WageType);
    }

    private async Task OnAddCategory()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var dialog = await DialogService.ShowAsync<Settings.Components.Categories.Editor>("افزودن دسته بندی جدید", dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: true })
        {
            await LoadCategoriesAsync();
            StateHasChanged();
        }
    }

    private async Task<IEnumerable<string>?> SearchNames(string? name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        await SendRequestAsync<IProductService, List<GetProductResponse>>(
            action: (s, ct) => s.GetListAsync(name, Model.ProductType, ct),
            afterSend: response => _products = response);

        return _products.Select(x => x.Name);
    }

    private void OnProductNameChanged(string? name)
    {
        var product = _products.FirstOrDefault(x => x.Name == name);

        if (product != null)
        {
            Model = ProductVm.CreateFromSearch(product);
            OnWageTypeChanged(product.WageType);
        }

        StateHasChanged();
    }
}