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

public partial class Create
{
    private readonly ProductVm _model = ProductVm.CreateDefaultInstance();
    private readonly ProductValidator _productValidator = new();
    private IEnumerable<ProductCategoryVm> _productCategories = [];
    private MudForm _form = default!;
    private bool _processing;
    private string? _wageHelperText;
    private string? _wageAdornmentText = "درصد";

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private IProductService ProductService => GetRequiredService<IProductService>();
    private IProductCategoryService CategoryService => GetRequiredService<IProductCategoryService>();

    protected override void OnInitialized()
    {
        GenerateBarcode();

        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadCategoriesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var response = await CategoryService.GetAllAsync(CancellationTokenSource.Token);

            _productCategories = response.Select(ProductCategoryVm.CreateFrom);
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

    private async Task Submit()
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

            var request = ProductVm.ToCreateRequest(_model);

            await ProductService.CreateAsync(request);

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

    private void GenerateBarcode() => _model.Barcode = StringExtensions.GenerateRandomBarcode();

    private void Close() => MudDialog.Cancel();

    private void OnProductTypeChanged(ProductType productType)
    {
        _model.ProductType = productType;

        switch (productType)
        {
            case ProductType.Jewelry:
                break;
            case ProductType.Gold:
                break;
            case ProductType.Coin:
                _model.Wage = null;
                _model.WageType = null;
                break;
            case ProductType.MoltenGold:
                _model.Wage = null;
                _model.WageType = null;
                break;
            case ProductType.UsedGold:
                _model.Wage = null;
                _model.WageType = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
    }

    private void OnWageChanged(double? wage)
    {
        _model.Wage = wage;

        _wageHelperText = _model.WageType switch
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
        _model.WageType = wageType;

        switch (wageType)
        {
            case WageType.Percent:
                _wageAdornmentText = "درصد";
                _wageHelperText = _model.Wage.HasValue ? $"{_model.Wage} درصد" : null;
                break;
            case WageType.Toman:
                _wageAdornmentText = "تومان";
                _wageHelperText = _model.Wage.HasValue ? $"{_model.Wage:N0} تومان" : null;
                break;
            case WageType.Dollar:
                _wageAdornmentText = "دلار";
                _wageHelperText = _model.Wage.HasValue ? $"{_model.Wage:N0} دلار" : null;
                break;
            case null:
                _wageAdornmentText = null;
                _wageHelperText = null;
                _model.Wage = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }
    }

    private void OnAddGemStone(MouseEventArgs obj)
    {
        _model.Stones ??= [];
        _model.Stones.Add(new GemStoneVm
        {
            Code = StringExtensions.GenerateRandomCode(5)
        });
        StateHasChanged();
    }

    private void OnRemoveGemStone(int index)
    {
        _model.Stones?.RemoveAt(index);
        StateHasChanged();
    }
}