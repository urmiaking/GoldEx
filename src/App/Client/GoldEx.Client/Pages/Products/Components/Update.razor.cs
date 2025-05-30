using GoldEx.Client.Pages.Products.Validators;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class Update
{
    [Parameter] public ProductVm Model { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly ProductValidator _productValidator = new();
    private MudForm _form = default!;
    private bool _processing;
    private string? _wageHelperText;
    private string? _wageAdornmentText = "درصد";
    private IEnumerable<ProductCategoryVm> _productCategories = [];

    private IProductService ProductService => GetRequiredService<IProductService>();
    private IProductCategoryService CategoryService => GetRequiredService<IProductCategoryService>();

    protected override void OnParametersSet()
    {
        OnWageChanged(Model.Wage);
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
        try
        {
            SetBusy();
            CancelToken();

            var response = await CategoryService.GetAllAsync(CancellationTokenSource.Token);

            _productCategories = response.Select(ProductCategoryVm.CreateFrom);

            StateHasChanged();
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }

    public async Task Submit()
    {
        await _form.Validate();

        if (!_form.IsValid)
            return;

        try
        {
            if (_processing)
                return;

            SetBusy();
            CancelToken();

            _processing = true;

            await ProductService.UpdateAsync(Model.Id, ProductVm.ToUpdateRequest(Model), CancellationTokenSource.Token);

            MudDialog.Close(DialogResult.Ok(true));
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
            _processing = false;
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
            case ProductType.Coin:
                Model.Wage = null;
                Model.WageType = null;
                break;
            case ProductType.MoltenGold:
                Model.Wage = null;
                Model.WageType = null;
                break;
            case ProductType.UsedGold:
                Model.Wage = null;
                Model.WageType = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
    }

    private void OnWageChanged(double? wage)
    {
        Model.Wage = wage;

        _wageHelperText = Model.WageType switch
        {
            WageType.Percent => $"{wage} درصد",
            WageType.Toman => $"{wage:N0} تومان",
            WageType.Dollar => $"{wage:N0} دلار",
            null => null,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void OnWageTypeChanged(WageType? wageType)
    {
        Model.WageType = wageType;

        switch (wageType)
        {
            case WageType.Percent:
                _wageAdornmentText = "درصد";
                _wageHelperText = Model.Wage.HasValue ? $"{Model.Wage} درصد" : null;
                break;
            case WageType.Toman:
                _wageAdornmentText = "تومان";
                _wageHelperText = Model.Wage.HasValue ? $"{Model.Wage:N0} تومان" : null;
                break;
            case WageType.Dollar:
                _wageAdornmentText = "دلار";
                _wageHelperText = Model.Wage.HasValue ? $"{Model.Wage:N0} دلار" : null;
                break;
            case null:
                _wageAdornmentText = null;
                _wageHelperText = null;
                Model.Wage = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }
    }

    private void OnProductCategoryChanged(ProductCategoryVm? category)
    {
        if (category is null)
            return;
        
        Model.CategoryVm = category;
        Model.ProductCategoryId = category.Id;
        Model.ProductCategoryTitle = category.Title;
    }

    private void OnRemoveGemStone(int index)
    {
        Model.Stones?.RemoveAt(index);
        StateHasChanged();
    }

    private void OnAddGemStone(MouseEventArgs obj)
    {
        Model.Stones ??= [];
        Model.Stones.Add(new GemStoneVm());
        StateHasChanged();
    }
}