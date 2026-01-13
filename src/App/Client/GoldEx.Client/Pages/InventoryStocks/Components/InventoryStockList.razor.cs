using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Client.Pages.Products.Components;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.DTOs.Settings.Barcodes;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Globalization;
using GoldEx.Client.Pages.Invoices.ViewModels;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryStockList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public string? SearchQuery { get; set; }
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public bool ShowTitle { get; set; }
    [Parameter] public bool Selectable { get; set; }
    [Parameter] public bool ShowItemStatus { get; set; } = true;
    [Parameter] public ItemType ItemType { get; set; }
    [Parameter] public ItemStatus ItemStatus { get; set; } = ItemStatus.Available;
    [Parameter] public Guid? InventoryEntryId { get; set; }
    [Parameter] public EventCallback<HashSet<InventoryStockVm>?> SelectedItemsChanged { get; set; }
    [Parameter] public ItemType[] SelectableTypes { get; set; } = Enum.GetValues<ItemType>();

    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private readonly string _jsVersion = new Random().Next(1, 1000).ToString();
    private MudTable<InventoryStockVm> _table = default!;

    private bool _mobileFiltersOpen;
    private DateRange _filterDateRange = new();
    private WarehouseActionType _actionType = WarehouseActionType.In;
    private List<ProductCategoryVm> _categories = [];
    private ProductCategoryVm? _categoryFilter;
    private GetBarcodePrintSettingsResponse? _barcodeSettings;
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

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
                    margin = x.BarcodeSettings.Margin
                }
                : null
        }).ToArray()
    };

    protected override void OnParametersSet()
    {
        if (ItemType == default) 
            ItemType = SelectableTypes.First();

        base.OnParametersSet();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await LoadCategoriesAsync();
            await LoadBarcodeSettingsAsync();
        }
        await base.OnAfterRenderAsync(firstRender);
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
            },
            createScope: false
        );

        StateHasChanged();
    }

    private async Task<TableData<InventoryStockVm>> LoadInventoryAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<InventoryStockVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, SearchQuery, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        var inventoryFilter = new InventoryFilter(_actionType, ItemType, _categoryFilter?.Id, _filterDateRange.Start, _filterDateRange.End, InventoryEntryId);

        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
            action: (s, token) => s.GetListAsync(filter, inventoryFilter, token),
            afterSend: response =>
            {
                var items = response.Data.Select(InventoryStockVm.CreateFrom).ToList();

                result = new TableData<InventoryStockVm>
                {
                    TotalItems = response.Total,
                    Items = items
                };
            },
            createScope: true,
            cancelPrevious: false
        );

        if (!string.IsNullOrEmpty(SearchQuery) && Guid.TryParse(SearchQuery, out _))
        {
            var item = result.Items?.FirstOrDefault();

            if (item is not null)
            {
                SetFilter(item);
            }
        }

        return result;
    }

    private void SetFilter(InventoryStockVm item)
    {
        if (item.Coin is not null)
        {
            ItemType = ItemType.Coin;
        }
        else if (item.Currency is not null)
        {
            ItemType = ItemType.Currency;
        }
        else if (item.Product is not null)
        {
            ItemType = ItemType.Product;
        }

        ItemStatus = item.CurrentAmount <= 0 ? ItemStatus.Sold : ItemStatus.Available;
    }

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task OnDateRangeChanged(DateRange dateRange)
    {
        _filterDateRange = dateRange;
        await RefreshAsync();
    }

    private async Task OnSearch(string text)
    {
        SearchQuery = text;

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);

        else
            await _table.ReloadServerData();
    }

    private void PageChanged(int i)
    {
        if (i <= 0)
            return;

        _table.NavigateTo(i - 1);
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
            productName = item.Product?.Name ?? "",
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

        if (ItemType is not ItemType.MoltenGold && ItemStatus is ItemStatus.Melted)
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

    private void ShowDetails(TableRowClickEventArgs<InventoryStockVm> args)
    {
        if (!Selectable)
        {
            var inventoryItem = _table.FilteredItems.FirstOrDefault(b => b.Equals(args.Item));
            if (inventoryItem is not null)
            {
                inventoryItem.ShowDetails = !inventoryItem.ShowDetails;
            }
        }
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

    private async Task OnEditProduct(InventoryStockVm context)
    {
        if (context.Product is null)
        {
            AddErrorToast("امکان ویرایش فقط برای طلا، جواهر و آبشده وجود دارد.");
            return;
        }

        var productModel = context.Product.Clone();
        productModel.Weight = context.CurrentAmount;

        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, productModel }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش جنس", parameters, _dialogOptions);

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

    private void OpenMobileFilters()
    {
        _mobileFiltersOpen = true;
        StateHasChanged();
    }

    private void CloseMobileFilters()
    {
        _mobileFiltersOpen = false;
        StateHasChanged();
    }

    private async Task ApplyMobileFilters(MobileFiltersResult result)
    {
        ItemType = result.ItemType;

        if (ItemType is not ItemType.MoltenGold && result.ItemStatus is ItemStatus.Melted)
        {
            ItemStatus = ItemStatus.Available;
        }
        else
        {
            ItemStatus = result.ItemStatus;
        }

        _categoryFilter = ItemType is ItemType.Product ? result.CategoryFilter : null;
        _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
        _actionType = ItemStatus == ItemStatus.Available ? WarehouseActionType.In : WarehouseActionType.Out;

        _mobileFiltersOpen = false;
        await RefreshAsync();
    }

    // Result model used by the popover to pass values
    public record MobileFiltersResult(
        ItemType ItemType,
        ItemStatus ItemStatus,
        ProductCategoryVm? CategoryFilter,
        DateRange DateRange
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
}