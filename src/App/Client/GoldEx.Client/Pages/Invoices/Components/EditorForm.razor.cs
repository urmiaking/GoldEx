using FluentValidation;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.DTOs.CoinInstances;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class EditorForm
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }
    [Parameter] public string? Barcode { get; set; }
    [Parameter] public TradeScale TradeScale { get; set; }
    [Parameter] public InvoiceType InvoiceType { get; set; }
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    private bool IsEditMode => Id.HasValue;

    private InvoiceVm _model = InvoiceVm.CreateDefaultInstance();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private readonly InvoiceValidator _invoiceValidator = new();
    private GetSettingResponse? _setting;
    private GetPriceResponse? _gramPrice;
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private List<GetCustomerResponse> _customers = [];
    private GetBarcodePrintSettingsResponse? _barcodeSettings;
    private GetLicenseResponse? _license;
    private bool _discountMenuOpen;
    private bool _extraCostsMenuOpen;
    private bool _processing;
    private bool _deleting;
    private bool _totalUnpaidMenuOpen;
    private bool _isLoadingInvoice;
    private bool _applyCurrentInvoice = true;
    private bool _paymentMenuOpen;

    private GetPriceUnitTitleResponse? DefaultPriceUnit =>
        _priceUnits.FirstOrDefault(x => x.IsDefault);

    private object SettingsForJs => new
    {
        labelWidth = _barcodeSettings?.LabelWidth,
        labelHeight = _barcodeSettings?.LabelHeight,
        tailWidth = _barcodeSettings?.TailWidth,
        marginTop = _barcodeSettings?.MarginTop,
        marginRight = _barcodeSettings?.MarginRight,
        marginBottom = _barcodeSettings?.MarginBottom,
        marginLeft = _barcodeSettings?.MarginLeft,
        paddingTop = _barcodeSettings?.PaddingTop,
        paddingRight = _barcodeSettings?.PaddingRight,
        paddingBottom = _barcodeSettings?.PaddingBottom,
        paddingLeft = _barcodeSettings?.PaddingLeft,
        positionItems = _barcodeSettings?.PositionItems.Select(x => new
        {
            position = x.Position.ToString(),
            itemType = x.ItemType.ToString(),
            order = x.Order,
            isVisible = x.IsVisible,
            fontSize = x.FontSize,
            itemSpacing = x.ItemSpacing,
            barcodeSettings = x.BarcodeSettings != null
                ? new
                {
                    width = x.BarcodeSettings.Width,
                    height = x.BarcodeSettings.Height,
                    displayValue = x.BarcodeSettings.DisplayValue,
                    fontSize = x.BarcodeSettings.FontSize,
                    margin = x.BarcodeSettings.Margin,
                    barWidthMultiplier = x.BarcodeSettings.BarWidthMultiplier
                }
                : null
        }).ToArray()
    };

    private string PrintUrl => ClientRoutes.Invoices.ViewInvoice.FormatRoute(new
    {
        number = _model.InvoiceNumber,
        invoiceType = _model.InvoiceType.ToString()
    });

    private Color InvoiceColor => _model.InvoiceType switch
    {
        InvoiceType.Sell => Color.Error,
        InvoiceType.Purchase => Color.Success,
        _ => Color.Default
    };

    private string RemainingTooltipText =>
        _applyCurrentInvoice
            ? "اعمال مانده فاکتور جاری در مانده مشتری"
            : "عدم اعمال مانده فاکتور جاری در مانده مشتری";

    private string InvoiceIcon => _model.InvoiceType switch
    {
        InvoiceType.Sell when _model.TradeScale is TradeScale.Retail => Icons.Material.Filled.ShoppingBasket,
        InvoiceType.Sell when _model.TradeScale is TradeScale.Wholesale => Icons.Material.Filled.ShoppingCart,
        InvoiceType.Purchase when _model.TradeScale is TradeScale.Retail => Icons.Material.Filled.ShoppingBasket,
        InvoiceType.Purchase when _model.TradeScale is TradeScale.Wholesale => Icons.Material.Filled.ShoppingCart,
        _ => Icons.Material.Filled.Receipt
    };

    private string InvoiceTableClass =>
        _model.InvoiceType switch
        {
            InvoiceType.Purchase => "invoice-table-purchase",
            InvoiceType.Sell => "invoice-table-sell",
            _ => ""
        };


    protected override async Task OnParametersSetAsync()
    {
        _isLoadingInvoice = true;
        _model.TradeScale = TradeScale;
        _model.InvoiceType = InvoiceType;

        await LoadPriceUnitsAsync();
        await LoadCustomerAsync();
        await LoadInvoiceAsync();
        await LoadSettingsAsync();
        await LoadGramPriceAsync();
        await LoadIncomingProductAsync();
        await LoadBarcodeSettingsAsync();
        await LoadLicenseAsync();
        SetHelpContext(_model.InvoiceType);

        _isLoadingInvoice = false;
        await base.OnParametersSetAsync();
    }

    private async Task LoadLicenseAsync()
    {
        await SendRequestAsync<ILicenseService, GetLicenseResponse>(
            action: (s, ct) => s.GetLicenseAsync(ct),
            afterSend: response => _license = response);
    }

    private async Task LoadBarcodeSettingsAsync()
    {
        await SendRequestAsync<IBarcodePrintSettingsService, GetBarcodePrintSettingsResponse>(
            action: (s, token) => s.GetAsync(token),
            afterSend: response => _barcodeSettings = response,
            createScope: true
        );
    }

    private async Task LoadIncomingProductAsync()
    {
        if (!string.IsNullOrEmpty(Barcode))
        {
            _model.InvoiceType = InvoiceType.Sell;
            await OnInvoiceTypeChanged(InvoiceType.Sell);

            await OnProductBarcodeChanged(Barcode);
            StateHasChanged();
        }
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
                    _model.CaptureOriginalUnpaidIfNeeded();
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
            afterSend: async response =>
            {
                _priceUnits = response;

                if (_model.InvoicePriceUnit is null)
                {
                    // Set default price unit to gram because the tradeScale initially set to wholesale
                    _model.InvoicePriceUnit = response.FirstOrDefault(x => TradeScale is TradeScale.Wholesale ? x.IsGoldBased : x.IsDefault);
                    await LoadExchangeRateAsync();
                }

                StateHasChanged();
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

    private async Task<IEnumerable<CustomerVm>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, null, ct),
            afterSend: response =>
            {
                _customers = response;
            },
            cancelPrevious: true);

        return _customers.Select(CustomerVm.CreateFrom);
    }

    private async Task OnAddCustomer()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true },
            { x => x.ShowFinancialAccounts, false }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن طرف حساب جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            _model.Customer = customerVm;
            StateHasChanged();
        }
    }

    private async Task OnEditCustomer()
    {
        if (_model.Customer is null)
            return;

        var parameters = new DialogParameters<Customers.Components.Editor>
            {
            { x => x.Model, _model.Customer },
            { x => x.ReturnModel, true },
            { x => x.ShowFinancialAccounts, true }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("ویرایش طرف حساب", parameters, _dialogOptions with { MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            _model.Customer = customerVm;
            StateHasChanged();
        }
    }

    private decimal GetNewInvoiceCustomerBalanceEffect() =>
        _model.InvoiceType == InvoiceType.Sell
            ? _model.TotalUnpaidAmount
            : -_model.TotalUnpaidAmount;

    private decimal GetCustomerBalanceChange() =>
        _model.InvoiceType == InvoiceType.Sell
            ? _model.UnpaidDelta
            : -_model.UnpaidDelta;

    #endregion

    #region ProductItem

    private async Task OnProductBarcodeChanged(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return;

        await SendRequestAsync<IProductService, GetProductResponse?>(
            action: async (s, ct) => await s.GetAsync(barcode, ct),
            afterSend: async response =>
            {
                if (response is null)
                    return;

                if (response.Weight is 0)
                {
                    AddErrorToast($"{response.Name} با کد {response.Barcode} قبلا فروخته شده است");
                    return;
                }

                decimal.TryParse(_gramPrice?.Value, out var gramPrice);

                decimal? wageExchangeRate = null;
                decimal? stoneExchangeRate = null;

                if (response.WagePriceUnitId.HasValue && response.WagePriceUnitId.Value != _model.InvoicePriceUnit?.Id)
                {
                    if (_model.InvoicePriceUnit != null)
                    {
                        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                            action: (s, ct) =>
                                s.GetExchangeRateAsync(response.WagePriceUnitId.Value, _model.InvoicePriceUnit.Id, ct),
                            afterSend: respExchangeRate =>
                            {
                                wageExchangeRate = respExchangeRate.ExchangeRate;
                            });
                    }
                }

                if (response.StonePriceUnit is not null && response.StonePriceUnit.Id != _model.InvoicePriceUnit?.Id)
                {
                    if (_model.InvoicePriceUnit != null)
                    {
                        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                            action: (s, ct) =>
                                s.GetExchangeRateAsync(response.StonePriceUnit.Id, _model.InvoicePriceUnit.Id, ct),
                            afterSend: respExchangeRate =>
                            {
                                stoneExchangeRate = respExchangeRate.ExchangeRate;
                            });
                    }
                }

                _model.ProductItems.Add(new ProductItemVm
                {
                    Product = ProductVm.CreateFrom(response),
                    TotalWeight = response.Weight,
                    GramPrice = gramPrice,
                    WageExchangeRate = wageExchangeRate,
                    StonePriceUnitExchangeRate = stoneExchangeRate,
                    CostPriceUnitId = _model.InvoicePriceUnit?.Id, // loading from invoice model
                    CostPriceUnitTitle = _model.InvoicePriceUnit?.Title,
                    InvoiceType = InvoiceType.Sell,
                    TaxPercent = _model.TradeScale is TradeScale.Retail ? _setting?.TaxPercent ?? 9 : 0,
                    ProfitPercent = _model.TradeScale is TradeScale.Retail ? response.ProductType == ProductType.Gold
                        ? _setting?.GoldProfitPercent ?? 7
                        : _setting?.JewelryProfitPercent ?? 20 : 0,
                    Index = _model.GetLastProductIndexNumber() + 1
                });
            },
            cancelPrevious: true);
    }

    private async Task OnOpenProductSelector()
    {
        decimal.TryParse(_gramPrice?.Value, out var gramPrice);

        var parameters = new DialogParameters<InventoryItemSelector>
        {
            { x => x.GramPrice, gramPrice },
            { x => x.TaxPercent, _model.TradeScale is TradeScale.Retail ? _setting?.TaxPercent ?? 10 : 0 },
            { x => x.GoldProfitPercent, _model.TradeScale is TradeScale.Retail ? _setting?.GoldProfitPercent ?? 7 : 0 },
            { x => x.JewelryProfitPercent, _model.TradeScale is TradeScale.Retail ? _setting?.JewelryProfitPercent ?? 20 : 0 },
            { x => x.SelectableTypes, [ItemType.Product, ItemType.MoltenGold ]},
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.ItemStatus, ItemStatus.Available }
        };

        var dialog = await DialogService.ShowAsync<InventoryItemSelector>("انتخاب جنس از انبار", parameters, _dialogOptions with { MaxWidth = MaxWidth.Large });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<ProductItemVm> productItems })
        {
            foreach (var item in productItems)
            {
                if (_model.ProductItems.All(x => x.Product.Id != item.Product.Id))
                {
                    item.Index = _model.GetLastProductIndexNumber() + 1;
                    _model.AddProductItem(item);
                }
            }
            StateHasChanged();
        }
    }

    private async Task OnAddProductItem()
    {
        var model = ProductItemVm.CreateDefaultInstance();

        decimal.TryParse(_gramPrice?.Value, out var gramPrice);

        model.GramPrice = gramPrice;
        model.TaxPercent = _model.InvoiceType is InvoiceType.Sell ? _setting?.TaxPercent ?? 9 : 0;
        model.ProfitPercent = _model.InvoiceType is InvoiceType.Sell ? _setting?.GoldProfitPercent ?? 7 : 0;
        model.IsInstantProduct = _model.InvoiceType is InvoiceType.Sell;
        model.InvoiceType = _model.InvoiceType;
        model.CostPriceUnitId = _model.InvoicePriceUnit?.Id;
        model.CostPriceUnitTitle = _model.InvoicePriceUnit?.Title;

        var parameters = new DialogParameters<ProductItemEditor>
        {
            { x => x.Model, model },
            { x => x.PriceUnits, _priceUnits },
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.TradeScale, _model.TradeScale }
        };

        var dialog = await DialogService.ShowAsync<ProductItemEditor>("افزودن جنس جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: ProductItemVm productItem })
        {
            // Reserve and assign barcode for brand-new items
            await AssignReservedBarcodeIfNeeded(BarcodeType.Product, productItem, null);

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
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.TradeScale, _model.TradeScale }
        };

        var dialog = await DialogService.ShowAsync<ProductItemEditor>("ویرایش جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: ProductItemVm resultItem })
        {
            productItemVm.UpdateFrom(resultItem);

            // Reserve and assign barcode if needed
            await AssignReservedBarcodeIfNeeded(BarcodeType.Product, productItemVm, null);

            StateHasChanged();
        }
    }

    private async Task OnRemoveProductItem(ProductItemVm productItem)
    {
        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {productItem.Product.Name} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        // Release reservation if this was a brand-new item with a reserved barcode
        await ReleaseReservedBarcodeIfNeeded(BarcodeType.Product, productItem, null);

        _model.RemoveProductItem(productItem);
    }

    private async Task AssignReservedBarcodeIfNeeded(BarcodeType barcodeType, ProductItemVm? productItem, CoinItemVm? coinItem)
    {
        switch (barcodeType)
        {
            case BarcodeType.Product:
                {
                    if (productItem is null)
                        return;

                    // Only for brand-new products
                    if (productItem.Product.Id.HasValue)
                        return;

                    if (!string.IsNullOrWhiteSpace(productItem.Product.Barcode))
                        return;

                    var request = new IssueNextBarcodeRequest(
                        BarcodeType.Product,
                        productItem.Product.ProductType,
                        productItem.Product.ProductCategoryId,
                        _model.InvoiceId);

                    await SendRequestAsync<IBarcodeReservationService, IssueNextBarcodeResponse>(
                        action: (svc, ct) => svc.IssueNextAsync(request, ct),
                        afterSend: resp =>
                        {
                            productItem.Product.Barcode = resp.Barcode;
                        });

                    break;
                }

            case BarcodeType.Coin:
                {
                    if (coinItem is null)
                        return;

                    // Only for instant coins (new CoinInstance)
                    if (coinItem.CoinInstance.Id.HasValue)
                        return;

                    if (!string.IsNullOrWhiteSpace(coinItem.CoinInstance.Barcode))
                        return;

                    var request = new IssueNextBarcodeRequest(
                        BarcodeType.Coin,
                        null,
                        null,
                        _model.InvoiceId);

                    await SendRequestAsync<IBarcodeReservationService, IssueNextBarcodeResponse>(
                        action: (svc, ct) => svc.IssueNextAsync(request, ct),
                        afterSend: resp =>
                        {
                            coinItem.CoinInstance.Barcode = resp.Barcode;
                        });

                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(barcodeType), barcodeType, null);
        }
    }

    private async Task ReleaseReservedBarcodeIfNeeded(BarcodeType barcodeType, ProductItemVm? productItem, CoinItemVm? coinItem)
    {
        switch (barcodeType)
        {
            case BarcodeType.Product:
                {
                    if (productItem is null)
                        return;

                    // Only for brand-new products
                    if (productItem.Product.Id.HasValue)
                        return;

                    if (string.IsNullOrWhiteSpace(productItem.Product.Barcode))
                        return;

                    await SendRequestAsync<IBarcodeReservationService>(
                        action: (svc, ct) =>
                            svc.ReleaseAsync(BarcodeType.Product, productItem.Product.Barcode, ct));

                    break;
                }

            case BarcodeType.Coin:
                {
                    if (coinItem is null)
                        return;

                    // Only for instant coins (new CoinInstance)
                    if (coinItem.CoinInstance.Id.HasValue)
                        return;

                    if (string.IsNullOrWhiteSpace(coinItem.CoinInstance.Barcode))
                        return;

                    await SendRequestAsync<IBarcodeReservationService>(
                        action: (svc, ct) =>
                            svc.ReleaseAsync(BarcodeType.Coin, coinItem.CoinInstance.Barcode, ct));

                    break;
                }

            default:
                throw new ArgumentOutOfRangeException(nameof(barcodeType), barcodeType, null);
        }
    }

    private async Task OnPrintProductBarcode(ProductItemVm item)
    {
        if (_barcodeSettings is null)
        {
            AddErrorToast("تنظیمات چاپ بارکد لود نشده است");
            return;
        }

        UnitType? wageUnitType = null;

        if (item.Product.WagePriceUnitId != null)
        {
            wageUnitType = await GetPriceUnitAsync(item.Product.WagePriceUnitId.Value);
        }

        var data = new
        {
            barcode = item.Product.Barcode ?? "",
            productName = item.Product.ProductType == ProductType.Jewelry && item.Product.Stones?.Any() == true
                ? GoldEx.Client.Pages.Products.Helpers.StonesClientHelper.GetStonesSummary(item.Product.Stones)
                : item.Product.Name ?? "",
            weight = $"W: {item.TotalWeight:G29}{(item.Product.GoldUnitType == GoldUnitType.Gram ? "G" : "M")}",
            wage = "F: " + item.Product.WageType switch
            {
                WageType.Fixed => $"{item.Product.Wage?.ToCurrencyFormat()} {wageUnitType?.ToString()}",
                WageType.Percent => $"{item.Product.Wage:G29}%",
                _ => "---"
            }
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", SettingsForJs, data);
    }

    #endregion

    #region CoinItem

    private async Task OnCoinBarcodeChanged(string barcode)
    {
        if (string.IsNullOrEmpty(barcode))
            return;

        await SendRequestAsync<ICoinInstanceService, GetCoinInstanceResponse?>(
            action: (s, ct) => s.GetAsync(barcode, ct),
            afterSend: async response =>
            {
                if (response is null)
                    return;

                var unitPrice = await GetCoinUnitPriceAsync(response.Coin.Id);

                _model.CoinItems.Add(new CoinItemVm
                {
                    CoinInstance = CoinInstanceVm.CreateFrom(response),
                    Quantity = 1,
                    UnitPrice = unitPrice
                });
            });
    }

    private async Task<decimal> GetCoinUnitPriceAsync(Guid coinId)
    {
        if (_model.InvoicePriceUnit is null)
            return 0;

        var result = await SendRequestAsync<ICoinService, GetExchangeRateResponse?>(
            action: (s, ct) => s.GetPriceAsync(coinId, _model.InvoicePriceUnit.IsDefault ? null : _model.InvoicePriceUnit.Id, ct));

        return result?.ExchangeRate ?? 0;
    }

    private async Task OnOpenCoinSelector()
    {
        var parameters = new DialogParameters<InventoryItemSelector>
        {
            { x => x.ItemType, ItemType.Coin },
            { x => x.SelectableTypes, [ItemType.Coin ]},
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.ItemStatus, ItemStatus.Available }
        };

        var dialog = await DialogService.ShowAsync<InventoryItemSelector>("انتخاب سکه از انبار", parameters, _dialogOptions with { MaxWidth = MaxWidth.Large });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<CoinItemVm> coinItems })
        {
            foreach (var item in coinItems)
            {
                if (_model.CoinItems.All(x => x.CoinInstance.Id != item.CoinInstance.Id))
                {
                    item.Index = _model.GetLastCoinIndexNumber() + 1;
                    _model.AddCoinItem(item);
                }
            }
            StateHasChanged();
        }
    }

    private async Task OnAddCoinItem()
    {
        var model = new CoinItemVm { Quantity = 1, IsInstant = _model.InvoiceType is InvoiceType.Sell };

        var parameters = new DialogParameters<CoinItemEditor>
        {
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.InvoiceType, _model.InvoiceType },
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("افزودن سکه جدید", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm coinItem })
        {
            await AssignReservedBarcodeIfNeeded(BarcodeType.Coin, null, coinItem);

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
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.InvoiceType, _model.InvoiceType }
        };

        var dialog = await DialogService.ShowAsync<CoinItemEditor>("ویرایش سکه", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CoinItemVm coinItem })
        {
            coinItemVm.UpdateFrom(coinItem);
            await AssignReservedBarcodeIfNeeded(BarcodeType.Coin, null, coinItem);
            StateHasChanged();
        }
    }

    private async Task OnRemoveCoinItem(CoinItemVm coinItem)
    {
        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {coinItem.CoinInstance.Coin?.Title} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        await ReleaseReservedBarcodeIfNeeded(BarcodeType.Coin, null, coinItem);

        _model.RemoveCoinItem(coinItem);
    }

    private async Task OnPrintCoinBarcode(CoinItemVm coinItem)
    {
        if (_barcodeSettings is null)
        {
            AddErrorToast("تنظیمات چاپ بارکد لود نشده است");
            return;
        }

        var data = new
        {
            barcode = coinItem.CoinInstance.Barcode,
            productName = coinItem.CoinInstance.Coin?.Title ?? "",
            weight = "",
            wage = ""
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", SettingsForJs, data);
    }

    #endregion

    #region CurrencyItem

    private async Task OnOpenCurrencySelector()
    {
        if (_license is null)
            return;

        if (_license.IsExpired)
        {
            AddErrorToast("قابلیت خرید و فروش ارز در نسخه Premium فعال است");
            return;
        }

        var parameters = new DialogParameters<InventoryItemSelector>
        {
            { x => x.ItemType, ItemType.Currency },
            { x => x.SelectableTypes, [ItemType.Currency ]},
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.ItemStatus, ItemStatus.Available },
        };

        var dialog = await DialogService.ShowAsync<InventoryItemSelector>("انتخاب ارز از انبار", parameters, _dialogOptions with { MaxWidth = MaxWidth.Medium });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<CurrencyItemVm> currencyItems })
        {
            foreach (var item in currencyItems)
            {
                if (_model.CurrencyItems.All(x => x.Currency.Id != item.Currency.Id))
                {
                    item.Index = _model.GetLastCurrencyIndexNumber() + 1;
                    _model.AddCurrencyItem(item);
                }
            }
            StateHasChanged();
        }
    }

    private async Task OnAddCurrencyItem()
    {
        if (_license is null)
            return;

        if (_license.IsExpired)
        {
            AddErrorToast("قابلیت خرید و فروش ارز در نسخه Premium فعال است");
            return;
        }

        var parameters = new DialogParameters<CurrencyItemEditor>
        {
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.InvoiceType, _model.InvoiceType }
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
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.InvoiceType, _model.InvoiceType }
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
        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {currencyItem.Currency.Title} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        _model.RemoveCurrencyItem(currencyItem);
    }

    #endregion

    #region UsedProducts

    private async Task OnEditUsedProduct(UsedProductVm usedProduct)
    {
        var parameters = new DialogParameters<UsedProductEditor>
        {
            { x => x.Model, usedProduct },
            { x => x.PriceUnit, _model.InvoicePriceUnit }
        };

        var dialog = await DialogService.ShowAsync<UsedProductEditor>("ویرایش جنس", parameters, _dialogOptions with { MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: UsedProductVm resultItem })
        {
            usedProduct.UpdateFrom(resultItem);
            StateHasChanged();
        }
    }

    private async Task OnRemoveUsedProduct(UsedProductVm usedProduct)
    {
        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف {usedProduct.Description} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        _model.RemoveUsedProduct(usedProduct);
    }

    private async Task OnAddUsedProduct()
    {
        if (_license is null)
            return;

        if (_license.IsExpired)
        {
            AddErrorToast("قابلیت طلای کهنه در نسخه Premium قابل استفاده است");
            return;
        }

        var model = new UsedProductVm();

        decimal.TryParse(_gramPrice?.Value, out var gramPrice);

        model.GramPrice = gramPrice;

        var parameters = new DialogParameters<UsedProductEditor>
        {
            { x => x.Model, model },
            { x => x.PriceUnit, _model.InvoicePriceUnit }
        };

        var dialog = await DialogService.ShowAsync<UsedProductEditor>("افزودن جنس جدید", parameters, _dialogOptions with { MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: UsedProductVm usedProduct })
        {
            usedProduct.RecalculateAmounts();
            _model.AddUsedProduct(usedProduct);
            StateHasChanged();
        }
    }

    private async Task OnPrintUsedBarcode(UsedProductVm item)
    {
        if (_barcodeSettings is null)
        {
            AddErrorToast("تنظیمات چاپ بارکد لود نشده است");
            return;
        }

        var data = new
        {
            barcode = item.Barcode,
            productName = item.Description ?? "",
            weight = $"W:{item.Weight:G29}{(item.UnitType == GoldUnitType.Gram ? "g" : "m")}",
            wage = "F:" + "---"
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", SettingsForJs, data);
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
                        item.WageExchangeRate = response.ExchangeRate;
                    });
            }
            else if (item.WageExchangeRate.HasValue)
            {
                item.WageExchangeRate = null;
            }
        }

        foreach (var coinItem in _model.CoinItems)
        {
            if (coinItem.CoinInstance.Coin != null)
            {
                coinItem.UnitPrice = await GetCoinUnitPriceAsync(coinItem.CoinInstance.Coin.Id);
            }
        }

        foreach (var currencyItem in _model.CurrencyItems)
        {
            if (currencyItem.Currency.Id != priceUnit.Id)
            {
                await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                    action: (s, ct) =>
                        s.GetExchangeRateAsync(currencyItem.Currency.Id, _model.InvoicePriceUnit.Id, ct),
                    afterSend: response =>
                    {
                        currencyItem.UnitPrice = response.ExchangeRate ?? 0;
                    });
            }
            else
            {
                currencyItem.UnitPrice = 1;
            }
        }

        foreach (var item in _model.UsedProducts)
        {
            decimal.TryParse(_gramPrice?.Value, out var gramPrice);
            item.GramPrice = gramPrice;
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

        if (_model is { UnpaidExchangeRate: not null, UnpaidPriceUnit: not null })
        {
            if (_model.InvoicePriceUnit.Id == _model.UnpaidPriceUnit.Id)
            {
                _model.UnpaidExchangeRate = null;
                _model.UnpaidPriceUnit = null;
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

    private async Task SubmitAsync(bool? skipPrint = false)
    {
        if (_processing)
            return;

        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        _processing = true;
        StateHasChanged();

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
            afterSend: async () =>
            {
                _processing = false;
                StateHasChanged();
                AddSuccessToast("فاکتور با موفقیت ذخیره شد");

                if (skipPrint == true)
                    return;

                var result = await DialogService.ShowMessageBoxAsync("چاپ فاکتور",
                    "آیا مایل به پرینت فاکتور هستید؟",
                    "بله، چاپ کن",
                    "خیر",
                    options: new DialogOptions { BackdropClick = false });

                Navigation.NavigateTo(result == true ? PrintUrl : ClientRoutes.Invoices.List);
            },
            onFailure: () =>
            {
                _processing = false;
                return Task.CompletedTask;
            });
    }

    private async Task OnInvoiceTypeChanged(InvoiceType invoiceType)
    {
        if (_model.ProductItems.Any() || _model.CoinItems.Any() || _model.CurrencyItems.Any())
        {
            var result = await DialogService.ShowMessageBoxAsync(
                "هشدار",
                markupMessage: new MarkupString("تغییر نوع فاکتور باعث حذف اقلام فاکتور خواهد شد. آیا مطمئن هستید؟"),
                yesText: "بله", cancelText: "لغو");
            if (result is null or false)
                return;

            _model.ProductItems.Clear();
            _model.CoinItems.Clear();
            _model.CurrencyItems.Clear();
        }

        if (invoiceType is InvoiceType.Sell)
        {
            var vouchers = _model.InvoicePayments.Where(x => x.VoucherId.HasValue).ToList();

            if (vouchers.Any())
            {
                var result = await DialogService.ShowMessageBoxAsync(
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
        await LoadGramPriceAsync();
        SetHelpContext(invoiceType);
        StateHasChanged();
    }

    private void SetHelpContext(InvoiceType invoiceType)
    {
        HelpContext.Slug = invoiceType switch
        {
            InvoiceType.Sell => "sell-invoice-video",
            InvoiceType.Purchase => "purchase-invoice-video",
            _ => null
        };
    }

    private async Task OnTradeScaleChanged(TradeScale tradeScale)
    {
        if (_model.ProductItems.Any() || _model.CoinItems.Any() || _model.CurrencyItems.Any())
        {
            if (tradeScale != TradeScale)
            {
                var result = await DialogService.ShowMessageBoxAsync(
                    "هشدار",
                    markupMessage:
                    new MarkupString("تغییر نوع معامله باعث حذف اقلام فاکتور خواهد شد. آیا مطمئن هستید؟"),
                    yesText: "بله", cancelText: "لغو");

                if (result is null or false)
                    return;

                _model.ProductItems.Clear();
                _model.CoinItems.Clear();
                _model.CurrencyItems.Clear();
            }
        }

        _model.TradeScale = tradeScale;

        var priceUnit = tradeScale switch
        {
            TradeScale.Retail => _priceUnits.FirstOrDefault(x => x.IsDefault),
            TradeScale.Wholesale => _priceUnits.FirstOrDefault(x => x.IsGoldBased),
            _ => null
        };

        await OnInvoicePriceUnitChanged(priceUnit);
    }

    private async Task OnPrintAsync()
    {
        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار", "آیا قبل از چاپ مایل به ذخیره تغییرات هستید؟",
            yesText: "بله",
            noText: "خیر",
            options: new DialogOptions { BackdropClick = false });

        if (result == true) 
            await SubmitAsync(skipPrint: true);

        // await JsRuntime.InvokeVoidAsync("open", PrintUrl, "_blank");
        Navigation.NavigateTo(PrintUrl);
    }

    private string FormatCompleteUnpaidAmount(decimal totalUnpaidAmount, string? primaryUnit, decimal? exchangeRate, decimal totalUnpaidSecondaryAmount, string? secondaryUnit)
    {
        var primaryAmount = Math.Abs(totalUnpaidAmount).ToCurrencyFormat(primaryUnit);
        var secondaryPart = exchangeRate.HasValue && !string.IsNullOrEmpty(secondaryUnit)
            ? $" ({Math.Abs(totalUnpaidSecondaryAmount).ToCurrencyFormat(secondaryUnit)})"
            : string.Empty;

        return $"{primaryAmount}{secondaryPart}";
    }

    private Color GetUnpaidAmountColor(decimal amount)
    {
        return amount switch
        {
            0 => Color.Primary,
            > 0 => Color.Error,
            _ => Color.Success
        };
    }

    private async Task HandleSendReminderClick()
    {
        if (!_model.InvoiceId.HasValue)
            return;

        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای ارسال پیامک تسویه حساب به شماره همراه {_model.Customer?.PhoneNumber} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        await SendRequestAsync<IInvoiceService>(
            action: (s, ct) => s.SendReminderAsync(_model.InvoiceId.Value, ct),
            afterSend: () =>
            {
                AddSuccessToast("پیامک با موفقیت ارسال شد");
                return Task.CompletedTask;
            });
    }

    private async Task OnDelete()
    {
        if (!_model.InvoiceId.HasValue)
            return;

        var result = await DialogService.ShowMessageBoxAsync(
            "هشدار",
            markupMessage: new MarkupString($"آیا برای حذف فاکتور شماره {_model.InvoiceNumber} اطمینان دارید؟ <br> <br> "),
            yesText: "بله", cancelText: "لغو");

        if (result is null)
            return;

        _deleting = true;

        await SendRequestAsync<IInvoiceService>(
            action: (s, ct) => s.DeleteAsync(_model.InvoiceId.Value, ct),
            afterSend: () =>
            {
                AddSuccessToast("فاکتور با موفقیت حذف شد");
                _deleting = false;
                Navigation.NavigateTo(ClientRoutes.Invoices.Index);
                return Task.CompletedTask;
            });
    }

    private async Task<UnitType?> GetPriceUnitAsync(Guid wagePriceUnitId)
    {
        GetPriceUnitResponse? priceUnit = null;

        await SendRequestAsync<IPriceUnitService, GetPriceUnitResponse>(
            action: (s, ct) => s.GetAsync(wagePriceUnitId, ct),
            afterSend: response =>
            {
                priceUnit = response;
            });

        return priceUnit?.UnitType;
    }

    #endregion

    #region MenuToggle

    private void OnDiscountMenuToggled()
    {
        _paymentMenuOpen = false;
        _discountMenuOpen = !_discountMenuOpen;
    }

    private void OnExtraCostsMenuToggled()
    {
        _paymentMenuOpen = false;
        _extraCostsMenuOpen = !_extraCostsMenuOpen;
    }

    private void OnTotalUnpaidMenuToggled()
    {
        _totalUnpaidMenuOpen = !_totalUnpaidMenuOpen;
    }

    #endregion

    #region Payments

    private async Task OpenPaymentsDialog()
    {
        var parameters = new DialogParameters<PaymentDialog>
        {
            { x => x.Items, _model.InvoicePayments.OrderBy(x => x.PaymentDate).ToList() },
            { x => x.PriceUnit, _model.InvoicePriceUnit },
            { x => x.InvoiceType, _model.InvoiceType },
            { x => x.CustomerId, _model.Customer?.Id },
            { x => x.PriceUnits, _priceUnits },
            { x => x.TotalRemaining, _model.TotalUnpaidAmount }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large,
            FullWidth = true
        };

        var dialog = await DialogService.ShowAsync<PaymentDialog>("جمع پرداختی", parameters, options);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: List<InvoicePaymentVm> payments })
        {
            _model.InvoicePayments = payments;
        }
    }

    private async Task OnAddPayment(PaymentType paymentType)
    {
        _paymentMenuOpen = false;

        if (_license is null)
            return;

        if (paymentType is PaymentType.CustomerTransfer && _license.IsExpired)
        {
            AddErrorToast("قابلیت پرداخت حواله ای در نسخه Premium قابل استفاده است");
            return;
        }

        var priceUnit = paymentType is PaymentType.MoltenGoldInventory or PaymentType.UsedGoldInventory
            ? _priceUnits.FirstOrDefault(pu => pu.IsGoldBased)
            : _model.InvoicePriceUnit;

        var adornmentText = priceUnit?.Title ?? string.Empty;

        var fineness = paymentType switch
        {
            PaymentType.MoltenGoldInventory => 750m,
            PaymentType.UsedGoldInventory => 750m - _setting?.UsedGoldFinenessDeductionRate,
            _ => null
        };

        var parameters = new DialogParameters<PaymentEditor>
        {
            {
                x => x.Model, new InvoicePaymentVm
                {
                    PaymentType = paymentType,
                    PriceUnit = priceUnit,
                    AmountAdornmentText = adornmentText,
                    GoldFineness = fineness,
                    PaymentSide = _model.InvoiceType is InvoiceType.Sell ? PaymentSide.Receive : PaymentSide.Pay
                }
            },
            { x => x.BasePriceUnit, _model.InvoicePriceUnit },
            { x => x.PriceUnits, _priceUnits },
            { x => x.InvoiceType, _model.InvoiceType },
            { x => x.TotalRemaining, _model.TotalUnpaidAmount }
        };

        var dialog = await DialogService.ShowAsync<PaymentEditor>(paymentType.GetDisplayTitle(), parameters, _dialogOptions with { MaxWidth = MaxWidth.Medium });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: InvoicePaymentVm addedPayment })
        {
            _model.InvoicePayments.Add(addedPayment);
        }
    }

    #endregion

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
    }
}