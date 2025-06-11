using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Extensions;
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
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly InvoiceItemValidator _invoiceItemValidator = new();
    private MudForm _form = default!;
    private string? _wageAdornmentText = "درصد";
    private string? _wageFieldHelperText;
    private IEnumerable<ProductCategoryVm> _productCategories = [];

    protected override void OnParametersSet()
    {
        if (Id is null)
            GenerateBarcode();

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

    private void OnWageTypeChanged(WageType? wageType)
    {
        Model.Product.WageType = wageType;

        switch (wageType)
        {
            case WageType.Percent:
                _wageAdornmentText = "درصد";
                break;
            case WageType.Fixed:
                _wageAdornmentText = "TODO"; // TODO: change to unit price
                break;
            case null:
                _wageAdornmentText = null;
                Model.Product.Wage = null;
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
        _wageFieldHelperText = Model.Product.Wage.HasValue || wage is not null
            ? $"{Model.Product.Wage.FormatNumber()} {_wageAdornmentText}".Trim()
            : string.Empty;
    }
}