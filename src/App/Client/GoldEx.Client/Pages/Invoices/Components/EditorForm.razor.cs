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
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class EditorForm
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }

    private bool IsEditMode => Id.HasValue;

    private InvoiceVm _model = InvoiceVm.CreateDefaultInstance();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private readonly InvoiceValidator _invoiceValidator = new();
    private GetSettingResponse? _setting;
    private GetPriceResponse? _gramPrice;
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private bool _isCustomerCreditLimitMenuOpen;
    private bool _discountMenuOpen;
    private bool _extraCostsMenuOpen;
    private bool _paymentsMenuOpen;
    private bool _processing;
    private bool _totalUnpaidMenuOpen;

    private string? CustomerCreditLimitAdornmentText => _model.Customer.CreditLimitPriceUnit?.Title;
    private GetPriceUnitTitleResponse? DefaultPriceUnit =>
        _priceUnits.FirstOrDefault(x => x.IsDefault);

    protected override async Task OnParametersSetAsync()
    {
        await LoadCustomerAsync();
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
            await LoadInvoiceNumberAsync();
        }
    }

    private async Task LoadInvoiceNumberAsync()
    {
        await SendRequestAsync<IInvoiceService, GetInvoiceNumberResponse>(
            action: (s, ct) => s.GetLastNumberAsync(_model.InvoiceType, ct),
            afterSend: response =>
            {
                _model.InvoiceNumber = response.InvoiceNumber + 1;
            });
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(GoldUnitType.Gram, _model.InvoicePriceUnit?.Id,
                _model.InvoiceType switch
                {
                    InvoiceType.Sell => true,
                    InvoiceType.Purchase => false,
                    _ => throw new ArgumentOutOfRangeException()
                }, ct),
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

                    StateHasChanged();
                }
            });
    }

    private async Task LoadCustomerAsync()
    {
        if (CustomerId.HasValue)
        {
            await SendRequestAsync<ICustomerService, GetCustomerResponse>(
                action: (s, ct) => s.GetAsync(CustomerId.Value, ct),
                afterSend: response => _model.Customer = CustomerVm.CreateFrom(response));
        }
    }

    #endregion

    #region Customer

    private void OnCustomerCreditLimitChanged(decimal? creditLimit)
    {
        _model.Customer.CreditLimit = creditLimit;
    }

    private void OnCreditLimitUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        _model.Customer.CreditLimitPriceUnit = priceUnit;
    }

    private void SelectCustomerCreditLimitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        OnCreditLimitUnitChanged(selectedUnit);
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

    private async Task OnCustomerPhoneNumberChanged(string phoneNumber)
    {
        _model.Customer.PhoneNumber = phoneNumber;

        if (string.IsNullOrEmpty(phoneNumber))
            return;

        await SendRequestAsync<ICustomerService, GetCustomerResponse?>(
            action: (s, ct) => s.GetByPhoneNumberAsync(phoneNumber, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                _model.Customer = CustomerVm.CreateFrom(response);
                OnCustomerCreditLimitChanged(response.CreditLimit);
            });
    }

    private void OnCustomerCleared()
    {
        _model.Customer = new CustomerVm { CreditLimitPriceUnit = _model.Customer.CreditLimitPriceUnit };
    }

    private void OnCustomerNationalIdAdornmentClicked()
    {
        _model.Customer.NationalId = StringExtensions.GenerateRandomCode(10);
        StateHasChanged();
    }

    #endregion

    #region Barcode

    private async Task OnBarcodeChanged(string barcode)
    {
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

                _model.ProductItems.Add(new ProductItemVm
                {
                    Product = ProductVm.CreateFrom(response),
                    GramPrice = gramPrice,
                    ExchangeRate = exchangeRate,
                    TaxPercent = _setting?.TaxPercent ?? 9,
                    ProfitPercent = response.ProductType == ProductType.Gold
                        ? _setting?.GoldProfitPercent ?? 7
                        : _setting?.JewelryProfitPercent ?? 20,
                    Index = _model.GetLastProductIndexNumber() + 1
                });
            });
    }

    #endregion

    #region ProductItem

    private async Task OnAddProductItem()
    {
        var model = ProductItemVm.CreateDefaultInstance();

        decimal.TryParse(_gramPrice?.Value, out var gramPrice);

        model.GramPrice = gramPrice;
        model.TaxPercent = _setting?.TaxPercent ?? 9;
        model.ProfitPercent = _setting?.GoldProfitPercent ?? 7;
        model.IsInstantProduct = _model.InvoiceType is InvoiceType.Sell;
        model.CostPriceUnitId = _model.InvoiceType is InvoiceType.Sell ? _model.InvoicePriceUnit?.Id : null;
        model.CostPriceUnitTitle = _model.InvoiceType is InvoiceType.Sell ? _model.InvoicePriceUnit?.Title : null;

        var parameters = new DialogParameters<ProductItemEditor>
        {
            { x => x.Model, model },
            { x => x.PriceUnits, _priceUnits },
            { x => x.PriceUnit, _model.InvoicePriceUnit }
        };

        var dialog = await DialogService.ShowAsync<ProductItemEditor>("افزودن جنس جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: ProductItemVm productItem })
        {
            productItem.RecalculateAmounts();
            _model.AddProductItem(productItem);
            StateHasChanged();
        }
    }

    private async Task OnEditProductItem(ProductItemVm productItemVm)
    {
        var parameters = new DialogParameters<ProductItemEditor>
        {
            { x => x.Model, productItemVm },
            { x => x.PriceUnits, _priceUnits },
            { x => x.PriceUnit, _model.InvoicePriceUnit }
        };

        var dialog = await DialogService.ShowAsync<ProductItemEditor>("ویرایش جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: ProductItemVm resultItem })
        {
            productItemVm.UpdateFrom(resultItem);
            StateHasChanged();
        }
    }

    private async Task OnRemoveProductItem(ProductItemVm productItem)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {productItem.Product.Name} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        _model.RemoveProductItem(productItem);
    }

    #endregion

    #region CoinItem

    private async Task OnAddCoinItem()
    {
        var parameters = new DialogParameters<CoinItemEditor>
        {
            { x => x.PriceUnit, _model.InvoicePriceUnit }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("افزودن سکه جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm coinItem })
        {
            coinItem.RecalculateAmounts();
            _model.AddCoinItem(coinItem);
            StateHasChanged();
        }
    }

    private async Task OnEditCoinItem(CoinItemVm coinItemVm)
    {
        var parameters = new DialogParameters<CoinItemEditor>
        {
            { x => x.Model, coinItemVm },
            { x => x.PriceUnit, _model.InvoicePriceUnit }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("ویرایش سکه", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm coinItem })
        {
            coinItemVm.UpdateFrom(coinItem);
            StateHasChanged();
        }
    }

    private async Task OnRemoveCoinItem(CoinItemVm coinItem)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {coinItem.Coin.Title} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        _model.RemoveCoinItem(coinItem);
    }

    #endregion

    #region CurrencyItem

    private async Task OnAddCurrencyItem()
    {
        var parameters = new DialogParameters<CurrencyItemEditor>
        {
            { x => x.PriceUnit, _model.InvoicePriceUnit }
        };

        var dialog = await DialogService.ShowAsync<CurrencyItemEditor>("افزودن ارز جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CurrencyItemVm currencyItem })
        {
            currencyItem.RecalculateAmounts();
            _model.AddCurrencyItem(currencyItem);
            StateHasChanged();
        }
    }

    private async Task OnEditCurrencyItem(CurrencyItemVm currencyItemVm)
    {
        var parameters = new DialogParameters<CurrencyItemEditor>
        {
            { x => x.Model, currencyItemVm },
            { x => x.PriceUnit, _model.InvoicePriceUnit }
        };
        var dialog = await DialogService.ShowAsync<CurrencyItemEditor>("ویرایش ارز", parameters, _dialogOptions);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: CurrencyItemVm resultItem })
        {
            currencyItemVm.UpdateFrom(resultItem);
            StateHasChanged();
        }
    }

    private async Task OnRemoveCurrencyItem(CurrencyItemVm currencyItem)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {currencyItem.Currency?.Title} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        _model.RemoveCurrencyItem(currencyItem);
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

        await LoadExchangeRateAsync();

        foreach (var item in _model.ProductItems)
        {
            decimal.TryParse(_gramPrice?.Value, out var gramPrice);
            item.GramPrice = gramPrice;

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
        }

        if (_model is { UnpaidExchangeRate: not null, UnpaidPriceUnit: not null })
        {
            if (_model.InvoicePriceUnit.Id == _model.UnpaidPriceUnit.Id)
            {
                _model.UnpaidExchangeRate = null;
                return;
            }

            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) =>
                    s.GetExchangeRateAsync(_model.InvoicePriceUnit.Id, _model.UnpaidPriceUnit.Id, ct),
                afterSend: response =>
                {
                    _model.UnpaidExchangeRate = response.ExchangeRate;
                });
        }

        StateHasChanged();
    }

    private async Task LoadExchangeRateAsync()
    {
        if (DefaultPriceUnit is null)
        {
            AddErrorToast("ارز پیش فرض تعریف نشده است");
            return;
        }

        if (_model.InvoicePriceUnit is null)
        {
            AddErrorToast("واحد ارزی فاکتور انتخاب نشده است");
            return;
        }

        if (_model.InvoicePriceUnit.Id == DefaultPriceUnit.Id)
        {
            _model.ExchangeRate = null;
            return;
        }

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) =>
                s.GetExchangeRateAsync(_model.InvoicePriceUnit.Id, DefaultPriceUnit.Id, ct),
            afterSend: response => _model.ExchangeRate = response.ExchangeRate);
    }

    private async Task SubmitAsync(string navigationUrl)
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
            action: (s, ct) => request.Id.HasValue ? s.UpdateAsync(request.Id.Value, request, ct) : s.CreateAsync(request, ct),
            afterSend: () =>
            {
                AddSuccessToast("فاکتور با موفقیت ذخیره شد");
                _processing = false;
                Navigation.NavigateTo(navigationUrl);
                return Task.CompletedTask;
            },
            onFailure: () =>
            {
                _processing = false;
                return Task.CompletedTask;
            });
    }

    private async Task SelectTotalUnpaidPriceUnit(GetPriceUnitTitleResponse? item)
    {
        _model.UnpaidPriceUnit = item;

        if (_model.InvoicePriceUnit is null)
            return;

        if (item is null)
        {
            _model.UnpaidExchangeRate = null;
            _totalUnpaidMenuOpen = false;
            return;
        }

        if (_model.InvoicePriceUnit.Id == item.Id)
        {
            _model.UnpaidExchangeRate = null;
            _totalUnpaidMenuOpen = false;
            return;
        }

        decimal? exchangeRate;

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) =>
                s.GetExchangeRateAsync(_model.InvoicePriceUnit.Id, item.Id, ct),
            afterSend: respExchangeRate =>
            {
                exchangeRate = respExchangeRate.ExchangeRate;
                _model.UnpaidExchangeRate = exchangeRate;
                _totalUnpaidMenuOpen = false;
            });
    }

    private async Task OnSubmitAndPrintAsync()
    {
        await SubmitAsync(ClientRoutes.Invoices.ViewInvoice.FormatRoute(new
        {
            number = _model.InvoiceNumber,
            invoiceType = _model.InvoiceType.ToString()
        }));
    }

    private async Task OnInvoiceTypeChanged(InvoiceType invoiceType)
    {
        if (invoiceType is InvoiceType.Sell)
        {
            var vouchers = _model.InvoicePayments.Where(x => x.VoucherId.HasValue).ToList();

            if (vouchers.Any())
            {
                var result = await DialogService.ShowMessageBox(
                    "هشدار",
                    markupMessage: new MarkupString("تغییر نوع فاکتور به فروش باعث حذف تمام رسیدهای پرداختی خواهد شد. آیا مطمئن هستید؟"),
                    yesText: "بله", cancelText: "لغو");
                if (result is null or false)
                    return;

                foreach (var voucher in vouchers)
                    _model.InvoicePayments.Remove(voucher);
            }
        }

        _model.InvoiceType = invoiceType;
        await LoadInvoiceNumberAsync();
    }

    private void OnPrint()
    {
        Navigation.NavigateTo(ClientRoutes.Invoices.ViewInvoice.FormatRoute(new
        {
            number = _model.InvoiceNumber,
            invoiceType = _model.InvoiceType.ToString()
        }));
    }

    #endregion

    #region MenuToggle

    private void OnDiscountMenuToggled()
    {
        _discountMenuOpen = !_discountMenuOpen;
    }

    private void OnExtraCostsMenuToggled()
    {
        _extraCostsMenuOpen = !_extraCostsMenuOpen;
    }

    private void OnPaymentsMenuToggled()
    {
        _paymentsMenuOpen = !_paymentsMenuOpen;
    }

    private void OnTotalUnpaidMenuToggled()
    {
        _totalUnpaidMenuOpen = !_totalUnpaidMenuOpen;
    }

    #endregion
}