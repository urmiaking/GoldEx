using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using GoldEx.Client.Pages.Products.ViewModels;
using Microsoft.AspNetCore.Components.Web;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class EditorForm
{
    [Parameter] public InvoiceVm Model { get; set; } = InvoiceVm.CreateDefaultInstance();

    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private readonly InvoiceValidator _invoiceValidator = new();
    private GetSettingResponse? _setting;
    private GetPriceResponse? _gramPrice;
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private bool _isCustomerCreditLimitMenuOpen;
    private string? _barcode;
    private string? _barcodeFieldHelperText;
    private InvoiceItemVm? _selectedInvoiceItem;
    private bool _discountMenuOpen;
    private bool _extraCostsMenuOpen;
    private bool _paymentsMenuOpen;


    protected override async Task OnParametersSetAsync()
    {
        await LoadPriceUnitsAsync();
        await LoadSettingsAsync();
        await LoadGramPrice();
        await base.OnParametersSetAsync();
    }

    private async Task LoadGramPrice()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(UnitType.Gold18K, ct),
            afterSend: response => _gramPrice = response);
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response => _setting = response);
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response => _priceUnits = response);
    }

    private void OnCustomerCreditLimitChanged(decimal? creditLimit)
    {
        Model.Customer.CreditLimit = creditLimit;
        Model.Customer.CreditLimitHelperText = $"{creditLimit.FormatNumber()} {Model.Customer.CreditLimitPriceUnit?.Title}".Trim();
    }

    private void OnCreditLimitUnitChanged(GetPriceUnitTitleResponse? unitType)
    {
        Model.Customer.CreditLimitPriceUnit = unitType;
        Model.Customer.CreditLimitHelperText = $"{Model.Customer.CreditLimit.FormatNumber()} {unitType?.Title}".Trim();
    }

    private void SelectCustomerCreditLimitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        OnCreditLimitUnitChanged(selectedUnit);
        Model.Customer.CreditLimitMenuOpen = false;
    }

    private async Task Submit()
    {
        if (IsBusy)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;
    }

    private async Task OnCustomerNationalIdChanged(string nationalId)
    {
        Model.Customer.NationalId = nationalId;

        await SendRequestAsync<ICustomerService, GetCustomerResponse?>(
            action: (s, ct) => s.GetAsync(nationalId, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                Model.Customer = CustomerVm.CreateFrom(response);
                OnCustomerCreditLimitChanged(response.CreditLimit);
            });
    }

    private async Task OnBarcodeChanged(string barcode)
    {
        _barcode = barcode;

        if (string.IsNullOrWhiteSpace(barcode))
        {
            OnBarcodeCleared();
            return;
        }

        await SendRequestAsync<IProductService, GetProductResponse?>(async (s, ct) => await s.GetAsync(barcode, ct),
            response =>
            {
                if (response is null)
                    return;

                decimal.TryParse(_gramPrice?.Value, out var gramPrice);

                Model.InvoiceItems.Add(new InvoiceItemVm
                {
                    Product = ProductVm.CreateFrom(response),
                    GramPrice = gramPrice,
                    TaxPercent = _setting?.TaxPercent ?? 9,
                    ProfitPercent = response.ProductType == ProductType.Gold
                        ? _setting?.GoldProfitPercent ?? 7
                        : _setting?.JewelryProfitPercent ?? 20,
                    Quantity = 1,
                    Index = Model.GetLastIndexNumber() + 1
                });

                OnBarcodeCleared();
            });
    }

    private void OnBarcodeCleared()
    {
        _barcode = null;
    }

    private Task OnEditInvoiceItem(InvoiceItemVm invoiceItem)
    {
        throw new NotImplementedException();
    }

    private async Task OnRemoveInvoiceItem(InvoiceItemVm invoiceItem)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {invoiceItem.Product.Name} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        Model.RemoveInvoiceItem(invoiceItem);
    }

    private async Task OnAddInvoiceItem()
    {
        var model = InvoiceItemVm.CreateDefaultInstance();
        decimal.TryParse(_gramPrice?.Value, out var gramPrice);

        model.GramPrice = gramPrice;
        model.TaxPercent = _setting?.TaxPercent ?? 9;
        model.ProfitPercent = _setting?.GoldProfitPercent ?? 7;

        var parameters = new DialogParameters<InvoiceItemEditor>
        {
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<InvoiceItemEditor>("افزودن جنس جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result?.Data is InvoiceItemVm invoiceItem)
        {
            Model.InvoiceItems.Add(invoiceItem);
            AddSuccessToast("جنس جدید با موفقیت افزوده شد.");
        }
    }

    private Task OnDiscountMenuToggled()
    {
        _discountMenuOpen = !_discountMenuOpen;
        return Task.CompletedTask;
    }

    private Task OnExtraCostsMenuToggled()
    {
        _extraCostsMenuOpen = !_extraCostsMenuOpen;
        return Task.CompletedTask;
    }

    private Task OnPaymentsMenuToggled()
    {
        _paymentsMenuOpen = !_paymentsMenuOpen;
        return Task.CompletedTask;
    }
}