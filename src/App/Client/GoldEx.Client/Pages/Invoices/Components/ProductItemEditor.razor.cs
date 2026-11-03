using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components.Web;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.Components.Categories;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.DTOs.StoneTypes;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class ProductItemEditor
{
    [Parameter] public ProductItemVm Model { get; set; } = ProductItemVm.CreateDefaultInstance();
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [Parameter] public TradeScale TradeScale { get; set; }
    [Parameter] public Func<ProductItemVm, Task<bool>>? PrepareAndAddAsync { get; set; }
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;

    private readonly ProductItemValidator _productItemValidator = new();

    private MudForm _form = default!;

    private IEnumerable<ProductCategoryVm> _productCategories = [];
    private List<GetProductResponse> _products = [];
    private GetSettingResponse? _settings;
    private List<GetStoneTypeResponse> _stoneTypes = [];

    // Keep editable display text separate from the decimal model for reliable live grouping.
    private string _gramPriceText = string.Empty;
    private decimal _displayedGramPrice;
    private bool _gramPriceTextInitialized;

    private bool _isProcessing;
    private List<GetCustomerResponse> _customers = [];

    private string? WageFieldAdornmentText => Model.Product.WageType switch
    {
        WageType.Percent => "درصد",
        WageType.Fixed => Model.Product.WagePriceUnitTitle,
        null => null,
        _ => throw new ArgumentOutOfRangeException()
    };

    private string? WageExchangeRateLabel => Model.Product.WageType switch
    {
        WageType.Percent => null,
        WageType.Fixed when PriceUnit.Id != Model.Product.WagePriceUnitId =>
            $"نرخ تبدیل {Model.Product.WagePriceUnitTitle} به {PriceUnit.Title}",
        null => null,
        _ => throw new ArgumentOutOfRangeException()
    };

    private string? StonePriceExchangeRateLabel => Model.Product.StonePriceUnit != null
                                                && PriceUnit.Id != Model.Product.StonePriceUnit.Id
        ? $"نرخ تبدیل {Model.Product.StonePriceUnit.Title} به {PriceUnit.Title}"
        : null;

    public string CostPriceAdornmentText => Model.CostPriceUnitTitle ?? PriceUnit.Title;
    public string? CostExchangeRateLabel => Model.CostPriceUnitId.HasValue && PriceUnit.Id != Model.CostPriceUnitId
        ? $"نرخ تبدیل {Model.CostPriceUnitTitle} به {PriceUnit.Title}"
        : null;

    protected override void OnParametersSet()
    {
        if (!_gramPriceTextInitialized || Model.GramPrice != _displayedGramPrice)
            SyncGramPriceText();
        if (!Model.Product.WagePriceUnitId.HasValue && Model.Product.WageType is WageType.Fixed)
        {
            Model.Product.WagePriceUnitId = PriceUnits.FirstOrDefault(x => x.IsDefault)?.Id;
            Model.Product.WagePriceUnitTitle = PriceUnits.FirstOrDefault(x => x.IsDefault)?.Title;
        }

        OnWageTypeChanged(Model.Product.WageType);

        // Profit/tax are initialized only when a new item is created in EditorForm.
        // Never reset these user-entered values during parameter updates or reopening
        // an unsaved wholesale item: its Id remains null until invoice persistence.

        base.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadCategoriesAsync();
        await LoadSettingsAsync();
        await LoadStoneTypesAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response => _settings = response);
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

    private void SyncGramPriceText()
    {
        _displayedGramPrice = Model.GramPrice;
        _gramPriceText = Model.GramPrice.ToString("#,##0.########################", CultureInfo.InvariantCulture);
        _gramPriceTextInitialized = true;
    }

    private void OnGramPriceTextChanged(string text)
    {
        _gramPriceText = GroupGramPriceDigits(text);
        if (TryParseGramPrice(text, out var price))
        {
            Model.GramPrice = price;
            _displayedGramPrice = price;
        }
    }

    private void OnGramPriceBlur(FocusEventArgs _)
    {
        if (TryParseGramPrice(_gramPriceText, out var price))
        {
            Model.GramPrice = price;
            SyncGramPriceText();
        }
    }

    private static IEnumerable<string> ValidateGramPriceText(string text)
    {
        if (!TryParseGramPrice(text, out var price) || price <= 0)
            return ["نرخ گرم باید بزرگتر از صفر باشد"];
        return [];
    }

    private static bool TryParseGramPrice(string? text, out decimal price)
    {
        var normalized = NormalizeGramPriceDigits(text).Replace(",", "", StringComparison.Ordinal);
        return decimal.TryParse(normalized,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture, out price);
    }

    private static string GroupGramPriceDigits(string? text)
    {
        var raw = NormalizeGramPriceDigits(text);
        var negative = raw.StartsWith("-", StringComparison.Ordinal);
        var unsigned = negative ? raw[1..] : raw;
        unsigned = unsigned.Replace(",", "", StringComparison.Ordinal);

        var decimalIndex = unsigned.IndexOf('.');
        var integer = decimalIndex >= 0 ? unsigned[..decimalIndex] : unsigned;
        var fraction = decimalIndex >= 0 ? unsigned[(decimalIndex + 1)..] : null;

        if (integer.Any(c => c is < '0' or > '9') ||
            (fraction is not null && fraction.Any(c => c is < '0' or > '9')))
            return text ?? string.Empty;

        var builder = new StringBuilder();
        if (negative) builder.Append('-');
        if (integer.Length == 0 && fraction is not null)
            builder.Append('0');

        for (var i = 0; i < integer.Length; i++)
        {
            if (i > 0 && (integer.Length - i) % 3 == 0) builder.Append(',');
            builder.Append(integer[i]);
        }
        if (fraction is not null) builder.Append('.').Append(fraction);
        return builder.ToString();
    }

    private static string NormalizeGramPriceDigits(string? text)
    {
        var builder = new StringBuilder();
        foreach (var ch in text ?? string.Empty)
        {
            if (ch is >= '۰' and <= '۹') builder.Append((char)('0' + ch - '۰'));
            else if (ch is >= '٠' and <= '٩') builder.Append((char)('0' + ch - '٠'));
            else if (ch == '٫') builder.Append('.');
            else if (ch is '٬' or '،') builder.Append(',');
            else builder.Append(ch);
        }
        return builder.ToString().Trim();
    }

    private async Task LoadGramPriceAsync()
    {
        await SendRequestAsync<IPriceService, GetPriceResponse?>(
            action: (s, ct) => s.GetAsync(Model.Product.GoldUnitType, PriceUnit.Id, true, ct),
            afterSend: response =>
            {
                decimal.TryParse(response?.Value, out var gramPriceValue);

                Model.GramPrice = gramPriceValue;
                SyncGramPriceText();

                StateHasChanged();
            });
    }

    private async Task Submit()
    {
        if (_isProcessing) return;

        _isProcessing = true;
        Session?.SetBusy(true);
        try
        {
            await _form.ValidateAsync();
            if (!_form.IsValid) return;

            // Prepare (including barcode reservation) and add to the invoice before
            // dismissing the editor, so the item is already in the list on close.
            if (PrepareAndAddAsync is not null && !await PrepareAndAddAsync(Model))
                return;

            if (Session is not null) Session.Close(Model);
            else MudDialog.Close(DialogResult.Ok(Model));
        }
        catch (Exception)
        {
            AddErrorToast("افزودن قلم انجام نشد. دوباره تلاش کنید.");
        }
        finally
        {
            _isProcessing = false;
            Session?.SetBusy(false);
        }
    }

    private void Close() { if (_isProcessing) return; if (Session is not null) Session.Cancel(); else MudDialog.Cancel(); }

    private void OnProductTypeChanged(ProductType productType)
    {
        Model.Product.ProductType = productType;

        switch (productType)
        {
            case ProductType.Jewelry:
                Model.SetAsJewelry(PriceUnit, _settings, TradeScale);
                break;
            case ProductType.Gold:
                Model.SetAsGold(PriceUnit, _settings, TradeScale);
                break;
            case ProductType.MoltenGold:
                Model.SetAsMoltenGold(_settings, TradeScale);
                break;
            case ProductType.UsedGold:
                throw new InvalidOperationException();
            default:
                throw new ArgumentOutOfRangeException(nameof(productType), productType, null);
        }
    }

    private void OnProductNameChanged(string? name)
    {
        if (Model.InvoiceType is InvoiceType.Purchase)
        {
            var product = _products.FirstOrDefault(x => x.Name == name);

            if (product != null)
            {
                Model.Product = ProductVm.CreateFromSearch(product);
                Model.TotalWeight = product.Weight;
                OnWageTypeChanged(product.WageType);
            }

            StateHasChanged();
        }

        Model.Product.Name = name;
    }

    private async Task<IEnumerable<string>?> SearchNames(string? name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
            return null;

        if (name.Length <= 2)
            return null;

        await SendRequestAsync<IProductService, List<GetProductResponse>>(
            action: (s, ct) => s.GetListAsync(name, Model.Product.ProductType, ct),
            afterSend: response => _products = response,
            createScope: true);

        return _products.Select(x => x.Name);
    }

    private async void OnWageTypeChanged(WageType? wageType)
    {
        Model.Product.WageType = wageType;

        switch (wageType)
        {
            case WageType.Percent:
                Model.WageExchangeRate = null;
                Model.Product.WagePriceUnitId = null;
                break;
            case WageType.Fixed:
                if (Model.Product.WagePriceUnitId.HasValue)
                {
                    await SelectWagePriceUnit(PriceUnits.First(x => x.Id == Model.Product.WagePriceUnitId));
                }
                else
                {
                    await SelectWagePriceUnit(PriceUnits.First(x => x.Id == PriceUnit.Id));
                }
                break;
            case null:
                Model.Product.Wage = null;
                Model.WageExchangeRate = null;
                Model.Product.WagePriceUnitId = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }

        StateHasChanged();
    }

    private void OnAddGemStone()
    {
        Model.Product.Stones ??= [];
        Model.Product.Stones.Add(new GemStoneVm());
        StateHasChanged();
    }

    private void OnRemoveGemStone(int index)
    {
        Model.Product.Stones?.RemoveAt(index);
        StateHasChanged();
    }

    private void OnProductCategoryChanged(ProductCategoryVm? category)
    {
        Model.Product.CategoryVm = category;
        Model.Product.ProductCategoryId = category?.Id;
        Model.Product.ProductCategoryTitle = category?.Title;
    }

    private async Task SelectWagePriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        Model.Product.WagePriceUnitId = priceUnit.Id;
        Model.Product.WagePriceUnitTitle = priceUnit.Title;

        if (PriceUnit.Id != Model.Product.WagePriceUnitId)
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(Model.Product.WagePriceUnitId.Value, PriceUnit.Id, ct),
                afterSend: response =>
                {
                    if (response.ExchangeRate.HasValue)
                        Model.WageExchangeRate = response.ExchangeRate.Value;
                });

        StateHasChanged();
    }

    private async Task SelectCostPriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        Model.CostPriceUnitId = priceUnit.Id;
        Model.CostPriceUnitTitle = priceUnit.Title;
        Model.CostPriceExchangeRate = null;

        if (PriceUnit.Id != Model.CostPriceUnitId)
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(Model.CostPriceUnitId.Value, PriceUnit.Id, ct),
                afterSend: response =>
                {
                    if (response.ExchangeRate.HasValue)
                        Model.CostPriceExchangeRate = response.ExchangeRate.Value;
                });

        StateHasChanged();
    }

    private async Task OnAddCategory()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var dialog = ResponsiveDialogs.Show<Editor>("افزودن دسته بندی جدید", wide: true);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: true })
        {
            await LoadCategoriesAsync();
            StateHasChanged();
        }
    }

    private async Task OnGoldUnitTypeSelected(GoldUnitType unitType)
    {
        Model.Product.GoldUnitType = unitType;
        await LoadGramPriceAsync();
    }

    private async Task OnProductStonePriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        Model.Product.StonePriceUnit = priceUnit;

        if (Model.Product.StonePriceUnit?.Id == PriceUnit.Id)
        {
            Model.StonePriceUnitExchangeRate = null;
        }
        else if (Model.Product.StonePriceUnit != null && PriceUnit.Id != Model.Product.StonePriceUnit.Id)
            await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
                action: (s, ct) => s.GetExchangeRateAsync(Model.Product.StonePriceUnit.Id, PriceUnit.Id, ct),
                afterSend: response =>
                {
                    if (response.ExchangeRate.HasValue)
                        Model.StonePriceUnitExchangeRate = response.ExchangeRate.Value;
                });

        StateHasChanged();
    }

    private void OnTotalWeightChanged(decimal? totalWeight)
    {
        Model.TotalWeight = totalWeight;
        if (Model.Product != null && Model.InvoiceType is not InvoiceType.Sell)
        {
            Model.Product.Weight = totalWeight;
        }
        StateHasChanged();
    }

    private void OnWageChanged(decimal? wage)
    {
        Model.Product.Wage = wage;
        StateHasChanged();
    }

    private async Task PurchaseWageTypeChanged(WageType? wageType)
    {
        Model.PurchaseWageType = wageType;

        switch (wageType)
        {
            case WageType.Percent:
                Model.WageExchangeRate = null;
                Model.Product.WagePriceUnitId = null;
                break;
            case WageType.Fixed:
                if (Model.Product.WagePriceUnitId.HasValue)
                {
                    await SelectWagePriceUnit(PriceUnits.First(x => x.Id == Model.Product.WagePriceUnitId));
                }
                else
                {
                    await SelectWagePriceUnit(PriceUnits.First(x => x.Id == PriceUnit.Id));
                }
                break;
            case null:
                Model.PurchaseWage = null;
                Model.WageExchangeRate = null;
                Model.Product.WagePriceUnitId = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(wageType), wageType, null);
        }

        StateHasChanged();
    }

    #region Customer

    private async Task<IEnumerable<CustomerVm>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, CustomerType.AssayingLab, ct),
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
            { x => x.CustomerType, CustomerType.AssayingLab }
        };

        var dialog = ResponsiveDialogs.Show<Customers.Components.Editor>("افزودن آزمایشگاه جدید",
            new Dictionary<string, object> {
                [nameof(Customers.Components.Editor.ReturnModel)] = true,
                [nameof(Customers.Components.Editor.CustomerType)] = CustomerType.AssayingLab,
                [nameof(Customers.Components.Editor.ShowFinancialAccounts)] = false
            }, wide: true);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            Model.Product.MoltenGold!.Assayer = customerVm;
            StateHasChanged();
        }
    }

    #endregion

    private void OnCostPriceChanged(decimal? costPrice)
    {
        Model.CostPrice = costPrice;

        if (Model.IsInstantProduct || Model.InvoiceType is InvoiceType.Purchase)
        {
            Model.FinalAmount = Model.CostPrice ?? 0;
        }
    }

    private void OnPurchaseWageChanged(decimal? val)
    {
        if (Model.ShowSaleWageMode)
        {
            OnWageChanged(val);
        }
        else
        {
            Model.PurchaseWage = val;

            if (Model.InvoiceType == InvoiceType.Purchase)
            {
                Model.SyncProductWageFromPurchase();
            }
        }
    }

    private async Task LoadStoneTypesAsync()
    {
        await SendRequestAsync<IStoneTypeService, List<GetStoneTypeResponse>>(
            action: (s, ct) => s.GetListAsync(new StoneTypeRequestFilter(true, null), ct),
            afterSend: response => _stoneTypes = response
        );
    }

    private void OnStoneTypeChanged(int index, Guid? val)
    {
        var stone = Model.Product.Stones?[index];
        stone?.StoneTypeId = val;
        var selectedType = _stoneTypes.FirstOrDefault(x => x.Id == val);
        if (selectedType != null)
        {
            stone?.Type = selectedType.Title;
            stone?.StoneTypeSymbol = selectedType.Symbol;
        }
    }
}