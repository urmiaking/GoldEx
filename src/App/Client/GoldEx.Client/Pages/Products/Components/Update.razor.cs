using GoldEx.Client.Pages.Products.Validators;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
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

    private IProductClientService ProductService => GetRequiredService<IProductClientService>();

    protected override void OnParametersSet()
    {
        OnWageChanged(Model.Wage);
        OnWageTypeChanged(Model.WageType);

        base.OnParametersSet();
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
            WageType.Rial => $"{wage:N0} ریال",
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
            case WageType.Rial:
                _wageAdornmentText = "ریال";
                _wageHelperText = Model.Wage.HasValue ? $"{Model.Wage:N0} ریال" : null;
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
}