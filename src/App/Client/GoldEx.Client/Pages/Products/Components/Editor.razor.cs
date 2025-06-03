using GoldEx.Client.Pages.Products.Validators;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using GoldEx.Sdk.Common.Extensions;

namespace GoldEx.Client.Pages.Products.Components;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public ProductVm Model { get; set; } = ProductVm.CreateDefaultInstance();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly ProductValidator _productValidator = new();
    private MudForm _form = default!;
    private bool _processing;
    private string? _wageAdornmentText = "درصد";
    private IEnumerable<ProductCategoryVm> _productCategories = [];

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
        if (_processing)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        _processing = true;

        if (Id is null)
        {
            var request = ProductVm.ToCreateRequest(Model);
            await SendRequestAsync<IProductService>((s, ct) => s.CreateAsync(request, ct));
        }
        else
        {
            var request = ProductVm.ToUpdateRequest(Model);
            await SendRequestAsync<IProductService>((s, ct) => s.UpdateAsync(Model.Id, request, ct));
        }

        _processing = false;

        MudDialog.Close(DialogResult.Ok(true));
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
                break;
            case WageType.Toman:
                _wageAdornmentText = "تومان";
                break;
            case WageType.Dollar:
                _wageAdornmentText = "دلار";
                break;
            case null:
                _wageAdornmentText = null;
                Model.Wage = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }
    }

    private void OnAddGemStone(MouseEventArgs obj)
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
}