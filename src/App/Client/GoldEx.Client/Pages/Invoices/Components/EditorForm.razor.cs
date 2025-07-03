using FluentValidation;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class EditorForm
{
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Parameter] public Guid? Id { get; set; }

    private InvoiceVm _model = InvoiceVm.CreateDefaultInstance();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private readonly InvoiceValidator _invoiceValidator = new();
    private GetSettingResponse? _setting;
    private GetPriceResponse? _gramPrice;
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private string? _barcode;
    private string? _barcodeFieldHelperText;
    private bool _isCustomerCreditLimitMenuOpen;
    private bool _discountMenuOpen;
    private bool _extraCostsMenuOpen;
    private bool _paymentsMenuOpen;
    private bool _processing;
    private string? _customerCreditLimitAdornmentText;

    protected override async Task OnParametersSetAsync()
    {
        await LoadInvoiceAsync();

        await LoadPriceUnitsAsync();
        await LoadSettingsAsync();
        await LoadGramPriceAsync();
        await base.OnParametersSetAsync();
    }

    #region Load Initial Data

    private async Task LoadInvoiceAsync()
    {
        if (Id.HasValue)
        {
            await SendRequestAsync<IInvoiceService, GetInvoiceResponse>(
                action: (s, ct) => s.GetAsync(Id.Value, ct),
                afterSend: response =>
                {
                    _model = InvoiceVm.CreateFrom(response);
                    OnCustomerCreditLimitChanged(_model.Customer.CreditLimit);
                });
        }
        else
        {
            await SendRequestAsync<IInvoiceService, GetInvoiceNumberResponse>(
                action: (s, ct) => s.GetLastNumberAsync(ct),
                afterSend: response =>
                {
                    _model.InvoiceNumber = response.InvoiceNumber + 1;
                });
        }
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(UnitType.Gold18K, _model.InvoicePriceUnit?.Id, true, ct),
            afterSend: response =>
            {
                _gramPrice = response;
            });
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
            afterSend: response =>
            {
                _priceUnits = response;

                if (_model.InvoicePriceUnit is null)
                {
                    _model.InvoicePriceUnit = response.FirstOrDefault(x => x.IsDefault);
                    _model.Customer.CreditLimitPriceUnit = response.FirstOrDefault(x => x.IsDefault);

                    _customerCreditLimitAdornmentText = _model.Customer.CreditLimitPriceUnit?.Title;

                    StateHasChanged();
                }
            });
    }

    #endregion

    #region Customer

    private void OnCustomerCreditLimitChanged(decimal? creditLimit)
    {
        _model.Customer.CreditLimit = creditLimit;
        _customerCreditLimitAdornmentText = _model.Customer.CreditLimitPriceUnit?.Title;
    }

    private void OnCreditLimitUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        _model.Customer.CreditLimitPriceUnit = priceUnit;
        _customerCreditLimitAdornmentText = priceUnit?.Title;
    }

    private void SelectCustomerCreditLimitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        OnCreditLimitUnitChanged(selectedUnit);
        _customerCreditLimitAdornmentText = selectedUnit.Title;
        _model.Customer.CreditLimitMenuOpen = false;
    }

    private async Task OnCustomerNationalIdChanged(string nationalId)
    {
        _model.Customer.NationalId = nationalId;

        if (string.IsNullOrEmpty(nationalId))
            return;

        await SendRequestAsync<ICustomerService, GetCustomerResponse?>(
            action: (s, ct) => s.GetAsync(nationalId, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                _model.Customer = CustomerVm.CreateFrom(response);
                OnCustomerCreditLimitChanged(response.CreditLimit);
            });
    }


    #endregion

    #region Barcode

    private async Task OnBarcodeChanged(string barcode)
    {
        _barcode = barcode;

        if (string.IsNullOrWhiteSpace(barcode))
        {
            OnBarcodeCleared();
            return;
        }

        await SendRequestAsync<IProductService, GetProductResponse?>(
            action: async (s, ct) => await s.GetAsync(barcode, false, ct),
            async response =>
            {
                if (response is null)
                    return;

                decimal.TryParse(_gramPrice?.Value, out var gramPrice);

                decimal? exchangeRate = null;

                if (response.WagePriceUnitId.HasValue && response.WagePriceUnitId.Value != _model.InvoicePriceUnit?.Id)
                {
                    if (_model.InvoicePriceUnit != null)
                    {
                        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                            action: (s, ct) =>
                                s.GetExchangeRateAsync(response.WagePriceUnitId.Value, _model.InvoicePriceUnit.Id, ct),
                            afterSend: respExchangeRate =>
                            {
                                exchangeRate = respExchangeRate.ExchangeRate;
                            });
                    }
                }

                _model.InvoiceItems.Add(new InvoiceItemVm
                {
                    Product = ProductVm.CreateFrom(response),
                    PriceUnit = _model.InvoicePriceUnit,
                    GramPrice = gramPrice,
                    ExchangeRate = exchangeRate,
                    TaxPercent = _setting?.TaxPercent ?? 9,
                    ProfitPercent = response.ProductType == ProductType.Gold
                        ? _setting?.GoldProfitPercent ?? 7
                        : _setting?.JewelryProfitPercent ?? 20,
                    Quantity = 1,
                    Index = _model.GetLastIndexNumber() + 1
                });

                OnBarcodeCleared();
            });
    }

    private void OnBarcodeCleared()
    {
        _barcode = null;
    }

    #endregion

    #region InvoiceItem

    private async Task OnEditInvoiceItem(InvoiceItemVm invoiceItemVm)
    {
        var parameters = new DialogParameters<InvoiceItemEditor>
        {
            { x => x.Model, invoiceItemVm },
            { x => x.PriceUnits, _priceUnits }
        };

        var dialog = await DialogService.ShowAsync<InvoiceItemEditor>("ویرایش جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: InvoiceItemVm resultItem })
        {
            invoiceItemVm.Copy(resultItem);
        }
    }

    private async Task OnRemoveInvoiceItem(InvoiceItemVm invoiceItem)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {invoiceItem.Product.Name} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        _model.RemoveInvoiceItem(invoiceItem);
    }

    private async Task OnAddInvoiceItem()
    {
        var model = InvoiceItemVm.CreateDefaultInstance();

        decimal.TryParse(_gramPrice?.Value, out var gramPrice);

        model.GramPrice = gramPrice;
        model.TaxPercent = _setting?.TaxPercent ?? 9;
        model.ProfitPercent = _setting?.GoldProfitPercent ?? 7;
        model.PriceUnit = _model.InvoicePriceUnit;

        var parameters = new DialogParameters<InvoiceItemEditor>
        {
            { x => x.Model, model },
            { x => x.PriceUnits, _priceUnits }
        };

        var dialog = await DialogService.ShowAsync<InvoiceItemEditor>("افزودن جنس جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: InvoiceItemVm invoiceItem })
        {
            invoiceItem.RecalculateAmounts();
            _model.InvoiceItems.Add(invoiceItem);
            StateHasChanged();
        }
    }

    #endregion

    #region Invoice

    private async Task OnInvoicePriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        if (priceUnit is null)
            return;

        _model.InvoicePriceUnit = priceUnit;

        if (_model.InvoicePriceUnit is null)
            return;

        await LoadGramPriceAsync();

        foreach (var item in _model.InvoiceItems)
        {
            decimal.TryParse(_gramPrice?.Value, out var gramPrice);
            item.GramPrice = gramPrice;

            item.PriceUnit = priceUnit;

            if (item.Product.WagePriceUnitId.HasValue && _model.InvoicePriceUnit.Id != item.Product.WagePriceUnitId)
            {
                await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                    action: (s, ct) =>
                        s.GetExchangeRateAsync(item.Product.WagePriceUnitId.Value, _model.InvoicePriceUnit.Id, ct),
                    afterSend: response =>
                    {
                        item.ExchangeRate = response.ExchangeRate;
                    });
            }
            else if (item.ExchangeRate.HasValue)
            {
                item.ExchangeRate = null;
            }
        }

        foreach (var item in _model.InvoiceDiscounts)
        {
            if (item.PriceUnit != null && item.PriceUnit.Id != priceUnit.Id)
            {
                await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                    action: (s, ct) =>
                        s.GetExchangeRateAsync(item.PriceUnit.Id, _model.InvoicePriceUnit.Id, ct),
                    afterSend: response =>
                    {
                        item.ExchangeRate = response.ExchangeRate;
                        item.ExchangeRateLabel = $"نرخ تبدیل {item.PriceUnit.Title} به {_model.InvoicePriceUnit.Title}";
                    });
            }
            else
            {
                item.ExchangeRate = null;
            }
        }

        foreach (var item in _model.InvoiceExtraCosts)
        {
            if (item.PriceUnit != null && item.PriceUnit.Id != priceUnit.Id)
            {
                await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                    action: (s, ct) =>
                        s.GetExchangeRateAsync(item.PriceUnit.Id, _model.InvoicePriceUnit.Id, ct),
                    afterSend: response =>
                    {
                        item.ExchangeRate = response.ExchangeRate;
                        item.ExchangeRateLabel = $"نرخ تبدیل {item.PriceUnit.Title} به {_model.InvoicePriceUnit.Title}";
                    });
            }
            else
            {
                item.ExchangeRate = null;
            }
        }

        foreach (var item in _model.InvoicePayments)
        {
            if (item.PriceUnit != null && item.PriceUnit.Id != priceUnit.Id)
            {
                await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                    action: (s, ct) =>
                        s.GetExchangeRateAsync(item.PriceUnit.Id, _model.InvoicePriceUnit.Id, ct),
                    afterSend: response =>
                    {
                        item.ExchangeRate = response.ExchangeRate;
                        item.ExchangeRateLabel = $"نرخ تبدیل {item.PriceUnit.Title} به {_model.InvoicePriceUnit.Title}";
                    });
            }
            else
            {
                item.ExchangeRate = null;
            }
        }

        if (_model.Customer.Id == null)
        {
            _model.Customer.CreditLimitPriceUnit = priceUnit;
            _customerCreditLimitAdornmentText = priceUnit.Title;
        }

        StateHasChanged();
    }

    #endregion

    #region MenuToggle

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

    #endregion

    private async Task Submit()
    {
        if (_processing)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        _processing = true;

        try
        {
            InvoiceVm.ToRequest(_model);
        }
        catch (ValidationException e)
        {
            _processing = false;
            AddErrorToast(e.Message);
            return;
        }

        var request = InvoiceVm.ToRequest(_model);

        await SendRequestAsync<IInvoiceService>(
            action: (s, ct) => s.SetAsync(request, ct),
            afterSend: () =>
            {
                AddSuccessToast("فاکتور با موفقیت ثبت شد");
                _processing = false;
                NavigationManager.NavigateTo(ClientRoutes.Invoices.Index);
                return Task.CompletedTask;
            });
    }

    private void OnCustomerCleared()
    {
        _model.Customer = new CustomerVm();
    }

    private void OnCustomerNationalIdAdornmentClicked()
    {
        _model.Customer.NationalId = StringExtensions.GenerateRandomCode(10);
        StateHasChanged();
    }
}