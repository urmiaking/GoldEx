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

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class EditorForm
{
    [Parameter] public InvoiceVm Model { get; set; } = InvoiceVm.CreateDefaultInstance();

    private readonly InvoiceValidator _invoiceValidator = new();
    private GetSettingResponse? _setting;
    private GetPriceResponse? _gramPrice;
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private bool _isCustomerCreditLimitMenuOpen;
    private string? _barcode;
    private string? _barcodeFieldHelperText;

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

                Model.AddInvoiceItem(response,
                    gramPrice,
                    null,
                    _setting?.TaxPercent,
                    response.ProductType == ProductType.Gold
                        ? _setting?.GoldProfitPercent
                        : _setting?.JewelryProfitPercent);
            });
    }

    private void OnBarcodeCleared()
    {
        _barcode = null;
    }
}