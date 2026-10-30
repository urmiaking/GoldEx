using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Products.Components;
using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.DTOs.Stores;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryDirectory
{
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;

    [Parameter] public string? SearchQuery { get; set; }
    [Parameter] public bool PersistState { get; set; } = true;
    [Parameter] public ItemType ItemType { get; set; } = ItemType.Product;
    [Parameter] public ItemStatus ItemStatus { get; set; } = ItemStatus.Available;
    [Parameter] public ItemType[] SelectableTypes { get; set; } = Enum.GetValues<ItemType>();
    [Parameter] public Guid? InventoryEntryId { get; set; }
    [Parameter] public Guid? InventoryExitId { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "page")] public int? PageQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "size")] public int? SizeQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "q")] public string? SearchQueryQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "type")] public string? TypeQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "category")] public string? CategoryQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "vitrine")] public bool? VitrineQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "start")] public string? StartQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "end")] public string? EndQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "sort")] public string? SortQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "asc")] public bool? AscendingQuery { get; set; }

    private List<InventoryStockVm> _items = [];
    private bool _initialized, _loading, _loadFailed;
    private string? _lastQuerySignature;
    private int _pageIndex, _pageSize = 10, _totalItems;
    private string _sortLabel = "CurrentQuantity";
    private Sdk.Common.Definitions.SortDirection _sortDirection = Sdk.Common.Definitions.SortDirection.Descending;
    private DateRange _filterDateRange = new();
    private WarehouseActionType _actionType = WarehouseActionType.In;
    private List<ProductCategoryVm> _categories = [];
    private ProductCategoryVm? _categoryFilter;
    private bool? _vitrineFilter;
    private GetBarcodePrintSettingsResponse? _barcodeSettings;
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Small };
    private ElementReference _directory, _filterBar;
    private IJSObjectReference? _stickyModule, _stickyTracker;

    private int PageCount => Math.Max(1, (_totalItems + _pageSize - 1) / _pageSize);
    private bool CanPrevious => !_loading && _pageIndex > 0;
    private bool CanNext => !_loading && _pageIndex + 1 < PageCount;
    private int ActiveFilterCount => (ItemType != ItemType.Product ? 1 : 0) +
        (ItemStatus != ItemStatus.Available ? 1 : 0) + (_categoryFilter is not null ? 1 : 0) +
        (_vitrineFilter.HasValue && ItemType == ItemType.Product ? 1 : 0) +
        ((_filterDateRange.Start.HasValue || _filterDateRange.End.HasValue) ? 1 : 0);

    private string DateRangeFilterLabel => ItemStatus switch
    {
        ItemStatus.Available => "تاریخ ثبت جنس",
        ItemStatus.Sold => "تاریخ فروش جنس",
        ItemStatus.Melted => "تاریخ ذوب جنس",
        _ => throw new ArgumentOutOfRangeException()
    };

    private string ItemStatusIcon => ItemStatus switch
    {
        ItemStatus.Available => Icons.Material.Filled.Warehouse,
        ItemStatus.Sold => Icons.Material.Filled.ShoppingBasket,
        ItemStatus.Melted => Icons.Material.Filled.Whatshot,
        _ => throw new ArgumentOutOfRangeException()
    };

    public string? ItemTypeIcon => ItemType switch
    {
        ItemType.Product => Icons.Material.Filled.Diamond,
        ItemType.Coin => Icons.Material.Filled.MonetizationOn,
        ItemType.Currency => Icons.Material.Filled.AttachMoney,
        ItemType.MoltenGold => Icons.Material.Filled.Whatshot,
        ItemType.UsedProduct => Icons.Material.Filled.DiscFull,
        _ => null
    };

    public Color ItemTypeColor => ItemType switch
    {
        ItemType.Product => Color.Info,
        ItemType.Coin => Color.Secondary,
        ItemType.Currency => Color.Tertiary,
        ItemType.MoltenGold => Color.Warning,
        ItemType.UsedProduct => Color.Error,
        _ => Color.Default
    };

    public Color ItemStatusColor => ItemStatus switch
    {
        ItemStatus.Available => Color.Success,
        ItemStatus.Sold => Color.Error,
        ItemStatus.Melted => Color.Warning,
        _ => Color.Default
    };

    private string GetItemTypeIcon(ItemType type) => type switch
    {
        ItemType.Product => Icons.Material.Filled.Diamond,
        ItemType.MoltenGold => Icons.Material.Filled.Whatshot,
        ItemType.UsedProduct => Icons.Material.Filled.DiscFull,
        ItemType.Coin => Icons.Material.Filled.MonetizationOn,
        ItemType.Currency => Icons.Material.Filled.AttachMoney,
        _ => Icons.Material.Filled.Help
    };

    private Color GetItemTypeIconColor(ItemType type) => type switch
    {
        ItemType.Product => Color.Info,
        ItemType.MoltenGold => Color.Warning,
        ItemType.UsedProduct => Color.Error,
        ItemType.Coin => Color.Primary,
        ItemType.Currency => Color.Tertiary,
        _ => Color.Default
    };

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

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        var signature = string.Join("|", PageQuery, SizeQuery, SearchQueryQuery, SearchQuery,
            TypeQuery, StatusQuery, CategoryQuery, VitrineQuery, StartQuery, EndQuery, SortQuery, AscendingQuery);
        if (_initialized && signature == _lastQuerySignature) return;

        // The list updates the URL after loading. A subsequent parameter update with
        // identical effective state must not trigger a duplicate server request.
        var previousPage = _pageIndex;
        var previousSize = _pageSize;
        var previousSearch = SearchQuery;
        var previousType = ItemType;
        var previousStatus = ItemStatus;
        var previousCategory = _categoryFilter?.Id;
        var previousVitrine = _vitrineFilter;
        var previousStart = _filterDateRange.Start;
        var previousEnd = _filterDateRange.End;
        var previousSort = _sortLabel;
        var previousDirection = _sortDirection;

        _lastQuerySignature = signature;
        _pageIndex = Math.Max(0, (PageQuery ?? 1) - 1);
        _pageSize = SizeQuery is 10 or 25 or 50 ? SizeQuery.Value : 10;
        SearchQuery = SearchQueryQuery ?? SearchQuery;

        var typeValue = ItemType.Product;
        if (!string.IsNullOrWhiteSpace(TypeQuery) && Enum.TryParse<ItemType>(TypeQuery, true, out var parsedType)
            && SelectableTypes.Contains(parsedType)) typeValue = parsedType;
        ItemType = typeValue;

        var statusValue = ItemStatus.Available;
        if (!string.IsNullOrWhiteSpace(StatusQuery) && Enum.TryParse<ItemStatus>(StatusQuery, true, out var parsedStatus))
            statusValue = parsedStatus;
        if (statusValue == ItemStatus.Melted && ItemType != ItemType.UsedProduct ||
            statusValue == ItemStatus.Sold && ItemType == ItemType.UsedProduct)
            statusValue = ItemStatus.Available;
        ItemStatus = statusValue;
        _actionType = ItemStatus == ItemStatus.Available ? WarehouseActionType.In : WarehouseActionType.Out;

        _categoryFilter = ItemType == ItemType.Product && Guid.TryParse(CategoryQuery, out var categoryId)
            ? _categories.FirstOrDefault(x => x.Id == categoryId) ?? new ProductCategoryVm { Id = categoryId }
            : null;
        _vitrineFilter = ItemType == ItemType.Product ? VitrineQuery : null;
        DateTime? ParseDate(string? value) => DateTime.TryParseExact(value, "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
        _filterDateRange = new DateRange(ParseDate(StartQuery), ParseDate(EndQuery));
        _sortLabel = SortQuery is "CurrentQuantity" or "SoldQuantity" or "Product.Barcode" or
            "Product.Name" or "Coin.Barcode" or "Currency.Title" ? SortQuery : "CurrentQuantity";
        _sortDirection = AscendingQuery == true ? Sdk.Common.Definitions.SortDirection.Ascending
            : Sdk.Common.Definitions.SortDirection.Descending;

        var changed = !_initialized || previousPage != _pageIndex || previousSize != _pageSize ||
            previousSearch != SearchQuery || previousType != ItemType || previousStatus != ItemStatus ||
            previousCategory != _categoryFilter?.Id || previousVitrine != _vitrineFilter ||
            previousStart != _filterDateRange.Start ||
            previousEnd != _filterDateRange.End || previousSort != _sortLabel ||
            previousDirection != _sortDirection;
        _initialized = true;
        if (changed) await LoadDirectoryAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;
        await LoadCategoriesAsync();
        await LoadBarcodeSettingsAsync();
        try
        {
            _stickyModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/invoice-sticky-header.js");
            _stickyTracker = await _stickyModule.InvokeAsync<IJSObjectReference>(
                "trackFilterHeight", _directory, _filterBar);
        }
        catch (JSDisconnectedException) { }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            if (_stickyTracker is not null)
            {
                await _stickyTracker.InvokeVoidAsync("dispose");
                await _stickyTracker.DisposeAsync();
            }
            if (_stickyModule is not null) await _stickyModule.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        finally { await base.DisposeAsync(); }
    }

    private async Task LoadBarcodeSettingsAsync()
    {
        await SendRequestAsync<IBarcodePrintSettingsService, GetBarcodePrintSettingsResponse>(
            action: (s, token) => s.GetAsync(token),
            afterSend: response => _barcodeSettings = response,
            createScope: true
        );
    }

    private async Task LoadCategoriesAsync()
    {
        await SendRequestAsync<IProductCategoryService, List<GetProductCategoryResponse>>(
            action: (s, token) => s.GetListAsync(token),
            afterSend: response =>
            {
                _categories = response.Select(ProductCategoryVm.CreateFrom).ToList();
                if (_categoryFilter != null && string.IsNullOrEmpty(_categoryFilter.Title))
                {
                    var fullCategory = _categories.FirstOrDefault(x => x.Id == _categoryFilter.Id);
                    if (fullCategory != null)
                    {
                        _categoryFilter = fullCategory;
                    }
                }
            },
            createScope: false
        );

        StateHasChanged();
    }

    private async Task LoadDirectoryAsync()
    {
        _loading = true;
        _loadFailed = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        try
        {
            var request = new RequestFilter(_pageIndex * _pageSize, _pageSize, SearchQuery,
                _sortLabel, _sortDirection);
            var inventory = new InventoryFilter(_actionType, ItemType, _categoryFilter?.Id,
                _filterDateRange.Start, _filterDateRange.End, InventoryEntryId, InventoryExitId,
                ItemType == ItemType.Product ? _vitrineFilter : null);
            await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
                action: (s, token) => s.GetListAsync(request, inventory, token),
                afterSend: response =>
                {
                    _items = response.Data.Select(InventoryStockVm.CreateFrom).ToList();
                    _totalItems = response.Total;
                },
                onFailure: () => _loadFailed = true,
                createScope: true, cancelPrevious: true);
            // Preserve existing deep links from barcode/QR scans. The service may
            // return a coin, currency, or product independent of the initial type.
            if (!_loadFailed && Guid.TryParse(SearchQuery, out _) && _items.FirstOrDefault() is { } matched)
                MatchSearchedItem(matched);
            if (!_loadFailed && _pageIndex >= PageCount)
            {
                _pageIndex = PageCount - 1;
                await LoadDirectoryAsync();
                return;
            }
            if (!_loadFailed && PersistState) UpdateUrl(_pageIndex);
        }
        finally { _loading = false; await InvokeAsync(StateHasChanged); }
    }

    private void MatchSearchedItem(InventoryStockVm item)
    {
        if (item.Coin is not null) ItemType = ItemType.Coin;
        else if (item.Currency is not null) ItemType = ItemType.Currency;
        else if (item.Product is not null) ItemType = ItemType.Product;

        ItemStatus = item.CurrentAmount <= 0 ? ItemStatus.Sold : ItemStatus.Available;
        _actionType = ItemStatus == ItemStatus.Available ? WarehouseActionType.In : WarehouseActionType.Out;
    }
    private async Task RefreshAsync()
    {
        _pageIndex = 0;
        await LoadDirectoryAsync();
    }
    private Task ReloadAsync() => LoadDirectoryAsync();
    private async Task OnSearch(string? text)
    {
        if (SearchQuery == text) return;
        SearchQuery = text;
        await RefreshAsync();
    }
    private async Task ChangePage(int delta)
    {
        var next = _pageIndex + delta;
        if (_loading || next < 0 || next >= PageCount) return;
        _pageIndex = next;
        await LoadDirectoryAsync();
    }
    private async Task ChangePageSize(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var size) || size is not (10 or 25 or 50)
            || size == _pageSize) return;
        _pageSize = size;
        await RefreshAsync();
    }
    private async Task ChangeSort(string label)
    {
        if (_sortLabel == label)
            _sortDirection = _sortDirection == Sdk.Common.Definitions.SortDirection.Descending
                ? Sdk.Common.Definitions.SortDirection.Ascending
                : Sdk.Common.Definitions.SortDirection.Descending;
        else _sortLabel = label;
        await RefreshAsync();
    }
    private async Task OnSortChanged(string label)
    {
        if (string.IsNullOrWhiteSpace(label) || _sortLabel == label) return;
        _sortLabel = label;
        await RefreshAsync();
    }
    private async Task ToggleSortDirection()
    {
        _sortDirection = _sortDirection == Sdk.Common.Definitions.SortDirection.Descending
            ? Sdk.Common.Definitions.SortDirection.Ascending
            : Sdk.Common.Definitions.SortDirection.Descending;
        await RefreshAsync();
    }
    private string SortIndicator(string label) => _sortLabel != label ? "↕" :
        _sortDirection == Sdk.Common.Definitions.SortDirection.Descending ? "↓" : "↑";
    private string SortTitle => _sortLabel switch
    {
        "CurrentQuantity" => "موجودی فعلی",
        "SoldQuantity" => "مقدار فروخته‌شده",
        "Product.Barcode" or "Coin.Barcode" => "بارکد",
        "Product.Name" => "نام جنس",
        "Currency.Title" => "عنوان ارز",
        _ => "موجودی فعلی"
    };
    private string FormattedAmount(decimal value, InventoryStockVm item) =>
        item.Product is not null ? value.ToWeightFormat(item.Product.GoldUnitType)
        : item.Coin is not null ? value.ToString("G29")
        : value.ToCurrencyFormat();
    private string ItemTitle(InventoryStockVm item) =>
        item.Product?.Name ?? (item.Coin is not null ? GetCoinTitle(item.Coin) : item.Currency?.Title) ?? "جنس بدون عنوان";
    private string ItemBarcode(InventoryStockVm item) =>
        item.Product?.Barcode ?? item.Coin?.Barcode ?? "";
    private string ItemSubTitle(InventoryStockVm item) =>
        item.Product?.ProductCategoryTitle ?? (item.Coin is not null ? GetCoinWeight(item.Coin) : "");
    private string GetWageDisplay(InventoryStockVm item)
    {
        var wage = ItemStatus == ItemStatus.Sold ? item.SaleWage : item.Product?.Wage;
        var wageType = ItemStatus == ItemStatus.Sold ? item.SaleWageType : item.Product?.WageType;
        var priceUnit = ItemStatus == ItemStatus.Sold ? item.SaleWagePriceUnitTitle : item.Product?.WagePriceUnitTitle;
        return wageType switch
        {
            WageType.Fixed => "اجرت: " + wage?.ToCurrencyFormat(priceUnit),
            WageType.Percent => $"اجرت: {wage:G29} درصد",
            _ => "بدون اجرت"
        };
    }
    private bool CanManageProduct(InventoryStockVm item) =>
        item.Product is not null && ItemType is ItemType.Product or ItemType.MoltenGold;
    private static bool CanShowVitrine(InventoryStockVm item) =>
        item.Product?.ProductType is ProductType.Gold or ProductType.Jewelry;
    private void ToggleDetails(InventoryStockVm item) => item.ShowDetails = !item.ShowDetails;
    private string QuantityStatus(InventoryStockVm item) =>
        item.CurrentAmount <= 0 ? "is-empty" : item.SoldAmount > 0 ? "is-partial" : "is-available";
    private async Task ResetAllFilters()
    {
        SearchQuery = null;
        ItemType = ItemType.Product;
        ItemStatus = ItemStatus.Available;
        _actionType = WarehouseActionType.In;
        _categoryFilter = null;
        _vitrineFilter = null;
        _filterDateRange = new DateRange();
        await RefreshAsync();
    }

    private async Task OnPrintProductBarcode(InventoryStockVm item)
    {
        UnitType? wageUnitType = null;

        if (item.Product?.WagePriceUnitId != null)
        {
            wageUnitType = await GetPriceUnitAsync(item.Product.WagePriceUnitId.Value);
        }

        var data = new
        {
            barcode = item.Product?.Barcode ?? "",
            productName = item.Product?.ProductType == ProductType.Jewelry && item.Product?.Stones?.Any() == true
                ? GoldEx.Client.Pages.Products.Helpers.StonesClientHelper.GetStonesSummary(item.Product.Stones)
                : item.Product?.Name ?? "",
            weight = $"W: {item.CurrentAmount:G29}{(item.Product?.GoldUnitType == GoldUnitType.Gram ? "G" : "M")}",
            wage = "F: " + item.Product?.WageType switch
            {
                WageType.Fixed => $"{item.Product?.Wage?.ToCurrencyFormat()} {wageUnitType?.ToString()}",
                WageType.Percent => $"{item.Product?.Wage:G29}%",
                _ => "---"
            }
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", SettingsForJs, data);
    }

    private async Task OnPrintCoinBarcode(InventoryStockVm item)
    {
        var data = new
        {
            barcode = item.Coin?.Barcode,
            productName = item.Coin?.Coin?.Title ?? "",
            weight = "",
            wage = ""
        };

        await JsRuntime.InvokeVoidAsync("printDynamicBarcode", SettingsForJs, data);
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

    private async Task SetStatusFilterText(ItemStatus filterType)
    {
        ItemStatus = filterType;
        _actionType = filterType == ItemStatus.Available
            ? WarehouseActionType.In
            : WarehouseActionType.Out;

        await RefreshAsync();
    }

    private async Task SetItemTypeFilterText(ItemType itemType)
    {
        ItemType = itemType;
        if (ItemType != ItemType.Product) _vitrineFilter = null;

        if (ItemType is not ItemType.UsedProduct && ItemStatus is ItemStatus.Melted)
        {
            ItemStatus = ItemStatus.Available;
        }

        await RefreshAsync();
    }

    private string GetBarcodeTooltipText(InventoryStockVm context)
    {
        return context.DateTime.ToString(CultureInfo.CurrentCulture);
    }

    private string? GetWageTooltipText(InventoryStockVm context)
    {
        if (context.SaleWage.HasValue)
        {
            return "اجرت خرید: " + context.PurchaseWageType switch
            {
                WageType.Fixed => $"{context.PurchaseWage?.ToCurrencyFormat(context.PurchaseWagePriceUnitTitle)}",
                WageType.Percent => context.PurchaseWage?.ToString("G29") + " درصد",
                _ => "ندارد"
            };
        }

        return null;
    }

    private async Task SetCategoryFilterText(ProductCategoryVm? category)
    {
        _categoryFilter = category;
        await RefreshAsync();
    }



    private Guid GetItemId(InventoryStockVm context)
    {
        if (context.Product is not null)
        {
            return context.Product.Id ?? throw new InvalidOperationException("Product ID is null.");
        }

        if (context.Coin is not null)
        {
            return context.Coin.Id ?? throw new InvalidOperationException("Coin ID is null.");
        }

        if (context.Currency is not null)
        {
            return context.Currency.Id;
        }

        throw new InvalidOperationException("Item ID could not be determined.");
    }

    private ItemType GetItemType(InventoryStockVm context)
    {
        if (context.Product is not null)
        {
            return ItemType.Product;
        }

        if (context.Coin is not null)
        {
            return ItemType.Coin;
        }

        if (context.Currency is not null)
        {
            return ItemType.Currency;
        }

        throw new InvalidOperationException("Item Type could not be determined.");
    }

    private async Task OnOpenVitrineDialog(InventoryStockVm context)
    {
        if (context.Product is null)
        {
            AddErrorToast("امکان مدیریت ویترین فقط برای مصنوعات طلا و جواهر وجود دارد.");
            return;
        }

        var productModel = context.Product.Clone();
        var parameters = new Dictionary<string, object>
        {
            [nameof(VitrineQuickEditDialog.Model)] = productModel
        };

        var dialog = ResponsiveDialogs.Show<VitrineQuickEditDialog>(
            "مدیریت ویترین آنلاین", parameters, wide: true);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: ProductVm updatedModel })
        {
            context.Product.ShowInVitrine = updatedModel.ShowInVitrine;
            context.Product.IsFeatured = updatedModel.IsFeatured;
            context.Product.VitrineDescription = updatedModel.VitrineDescription;
            context.Product.Images = updatedModel.Images;
            await RefreshAsync();
        }
    }

    private UserStoreDto? _currentStore;

    private async Task OnCopyProductVitrineUrl(InventoryStockVm context)
    {
        if (context.Product is null || string.IsNullOrWhiteSpace(context.Product.Barcode)) return;

        if (_currentStore == null)
        {
            var stores = await SendRequestAsync<IStoreService, List<UserStoreDto>>(
                action: (service, token) => service.GetUserStoresAsync(token));
            _currentStore = stores?.FirstOrDefault(s => s.IsCurrent) ?? stores?.FirstOrDefault();
        }

        var url = VitrineUrlHelper.BuildProductVitrineUrl(
            _currentStore?.CustomDomain,
            Navigation.BaseUri,
            _currentStore?.Slug ?? "default",
            context.Product.Barcode);

        await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", url);
        AddSuccessToast($"لینک عمومی کالا کپی شد: {url}");
    }

    private async Task OnEditProduct(InventoryStockVm context)
    {
        if (context.Product is null)
        {
            AddErrorToast("امکان ویرایش فقط برای طلا، جواهر و آبشده وجود دارد.");
            return;
        }

        var productModel = context.Product.Clone();
        productModel.Weight = context.CurrentAmount;

        var parameters = new Dictionary<string, object>
        {
            [nameof(Editor.Model)] = productModel
        };

        var dialog = ResponsiveDialogs.Show<Editor>("ویرایش جنس", parameters, wide: true);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس با موفقیت ویرایش شد.");
            await RefreshAsync();
        }
    }

    private async Task OnRemove(InventoryStockVm context)
    {
        if (context.Product is null)
        {
            AddErrorToast("امکان حذف فقط برای طلا، جواهر و آبشده وجود دارد.");
            return;
        }

        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, context.Product.Id!.Value },
            { x => x.ProductName, context.Product.Name }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف جنس", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("جنس با موفقیت حذف شد.");
            await RefreshAsync();
        }
    }

    private async Task OpenMobileFilters()
    {
        var parameters = new Dictionary<string, object>
        {
            { nameof(InventoryDirectoryFiltersDialog.ItemType), ItemType },
            { nameof(InventoryDirectoryFiltersDialog.ItemStatus), ItemStatus },
            { nameof(InventoryDirectoryFiltersDialog.CategoryFilter), _categoryFilter! },
            { nameof(InventoryDirectoryFiltersDialog.VitrineFilter), _vitrineFilter! },
            { nameof(InventoryDirectoryFiltersDialog.Categories), _categories },
            { nameof(InventoryDirectoryFiltersDialog.DateRange), _filterDateRange },
            { nameof(InventoryDirectoryFiltersDialog.SortLabel), _sortLabel },
            { nameof(InventoryDirectoryFiltersDialog.SortDescending),
                _sortDirection == Sdk.Common.Definitions.SortDirection.Descending }
        };
        var dialog = ResponsiveDialogs.Show<InventoryDirectoryFiltersDialog>("فیلتر موجودی انبار", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: MobileFiltersResult filters })
            await ApplyMobileFilters(filters);
    }

    private async Task ApplyMobileFilters(MobileFiltersResult result)
    {
        ItemType = result.ItemType;

        if (ItemType is not ItemType.UsedProduct && result.ItemStatus is ItemStatus.Melted)
        {
            ItemStatus = ItemStatus.Available;
        }
        else
        {
            ItemStatus = result.ItemStatus;
        }

        _categoryFilter = ItemType is ItemType.Product ? result.CategoryFilter : null;
        _vitrineFilter = ItemType is ItemType.Product ? result.VitrineFilter : null;
        _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
        _actionType = ItemStatus == ItemStatus.Available ? WarehouseActionType.In : WarehouseActionType.Out;

        _sortLabel = result.SortLabel;
        _sortDirection = result.SortDescending ? Sdk.Common.Definitions.SortDirection.Descending : Sdk.Common.Definitions.SortDirection.Ascending;
        await RefreshAsync();
    }

    // Result model used by the popover to pass values
    public record MobileFiltersResult(
        ItemType ItemType,
        ItemStatus ItemStatus,
        ProductCategoryVm? CategoryFilter,
        bool? VitrineFilter,
        DateRange DateRange,
        string SortLabel,
        bool SortDescending
    );

    private string GetCoinTitle(CoinInstanceVm coinInstance)
    {
        var coin = coinInstance.Coin;

        if (coin is null)
            return "سکه نامشخص";

        var baseTitle = coin.Title;

        var issuer = coinInstance.CoinPackage?.Issuer;
        if (issuer is null)
            return baseTitle;

        var issuerName = issuer.FullName;
        var nationalCode = issuer.NationalId;

        if (string.IsNullOrWhiteSpace(issuerName) || string.IsNullOrWhiteSpace(nationalCode))
            return baseTitle;

        return $"{baseTitle} - {issuerName} ({nationalCode})";
    }

    private string GetCoinWeight(CoinInstanceVm coinInstance)
    {
        var weight = coinInstance.Weight?.ToWeightFormat(GoldUnitType.Gram);

        var vacuumedWeight = coinInstance.CoinPackage?.VacuumedWeight?.ToWeightFormat(GoldUnitType.Gram);

        return vacuumedWeight is not null
            ? $"{weight} ({vacuumedWeight} با پرس)"
            : weight ?? "-";
    }

    private string GetCoinMintYear(CoinInstanceVm coinItem)
    {
        var mintYear = coinItem.MintYear;

        if (!mintYear.HasValue)
            return "نامشخص";

        var persianYear = new PersianCalendar().GetYear(mintYear.Value);

        return persianYear.ToString();
    }

    private void UpdateUrl(int pageIndex)
    {
        if (!PersistState)
            return;

        var parameters = new Dictionary<string, object?>();

        if (pageIndex > 0)
            parameters.Add("page", pageIndex + 1);
        if (_pageSize != 10)
            parameters.Add("size", _pageSize);
        if (_sortLabel != "CurrentQuantity")
            parameters.Add("sort", _sortLabel);
        if (_sortDirection == Sdk.Common.Definitions.SortDirection.Ascending)
            parameters.Add("asc", true);

        if (!string.IsNullOrWhiteSpace(SearchQuery))
            parameters.Add("q", SearchQuery);

        if (ItemType != default)
            parameters.Add("type", ItemType.ToString());

        if (ItemStatus != default)
            parameters.Add("status", ItemStatus.ToString());

        if (_categoryFilter != null)
            parameters.Add("category", _categoryFilter.Id);
        if (ItemType == ItemType.Product && _vitrineFilter.HasValue)
            parameters.Add("vitrine", _vitrineFilter.Value);

        if (_filterDateRange?.Start.HasValue == true)
            parameters.Add("start", _filterDateRange.Start.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        if (_filterDateRange?.End.HasValue == true)
            parameters.Add("end", _filterDateRange.End.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        var baseUrl = Navigation.Uri.Split('?')[0];
        var newUrl = baseUrl.AppendQueryString(parameters);

        if (Navigation.Uri != newUrl)
        {
            Navigation.NavigateTo(newUrl, replace: true);
        }
    }
}