using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Products.Validators;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public ProductVm Model { get; set; } = ProductVm.CreateDefaultInstance();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly ProductValidator _productValidator = new();
    private IEnumerable<ProductCategoryVm> _productCategories = [];
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private string? _wageFieldHelperText;
    private string? _wageAdornmentText = "درصد";
    private string? _wageFieldAdornmentText;
    private bool _wageFieldMenuOpen;
    private MudForm _form = default!;

    protected override void OnParametersSet()
    {
        if (Id is null)
            GenerateBarcode();

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

                var defaultUnit = _priceUnits.FirstOrDefault(x => x.IsDefault);

                Model.WagePriceUnitId ??= defaultUnit?.Id;
                Model.WagePriceUnitTitle ??= defaultUnit?.Title;
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

        if (Id is null)
        {
            var request = ProductVm.ToCreateRequest(Model);
            await SendRequestAsync<IProductService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            var request = ProductVm.ToUpdateRequest(Model);
            await SendRequestAsync<IProductService>(
                action: (s, ct) => s.UpdateAsync(Model.Id, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
    }

    private void GenerateBarcode() => Model.Barcode = StringExtensions.GenerateRandomBarcode();

    private void Close() => MudDialog.Cancel();

    private void OnProductTypeChanged(ProductType productType)
    {
        Model.ProductType = productType;

        switch (productType)
        {
            case ProductType.Jewelry:
                break;
            case ProductType.Gold:
                break;
            case ProductType.MoltenGold:
                Model.Wage = null;
                Model.WageType = null;
                break;
            case ProductType.OldGold:
                Model.Wage = null;
                Model.WageType = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
    }

    private void OnWageTypeChanged(WageType? wageType)
    {
        Model.WageType = wageType;

        switch (wageType)
        {
            case WageType.Percent:
                _wageAdornmentText = "درصد";
                _wageFieldAdornmentText = "درصد";
                break;
            case WageType.Fixed:
                _wageAdornmentText = Model.WagePriceUnitTitle;
                _wageFieldAdornmentText = Model.WagePriceUnitTitle;
                break;
            case null:
                _wageAdornmentText = null;
                Model.Wage = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }

        OnWageChanged(Model.Wage);
    }

    private void OnAddGemStone()
    {
        Model.Stones ??= [];
        Model.Stones.Add(new GemStoneVm
        {
            Code = StringExtensions.GenerateRandomCode(5)
        });
        StateHasChanged();
    }

    private void OnRemoveGemStone(int index)
    {
        Model.Stones?.RemoveAt(index);
        StateHasChanged();
    }

    private void OnProductCategoryChanged(ProductCategoryVm? category)
    {
        if (category is null)
            return;

        Model.CategoryVm = category;
        Model.ProductCategoryId = category.Id;
        Model.ProductCategoryTitle = category.Title;
    }

    private void OnWageChanged(decimal? wage)
    {
        Model.Wage = wage;
        _wageFieldHelperText = Model.Wage.HasValue || wage is not null
            ? $"{Model.Wage.FormatNumber()} {_wageAdornmentText}".Trim()
            : string.Empty;
    }

    private void OnWageAdornmentClicked()
    {
        if (Model.WageType is WageType.Fixed)
        {
            _wageFieldMenuOpen = !_wageFieldMenuOpen;
        }
    }

    private void SelectWagePriceUnit(GetPriceUnitTitleResponse item)
    {
        Model.WagePriceUnitId = item.Id;
        Model.WagePriceUnitTitle = item.Title;

        _wageFieldMenuOpen = false;
        OnWageTypeChanged(Model.WageType);
    }
}