using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

/// <summary>
/// Shared invoice, melting and kardex selector. Selections survive server-side paging.
/// Changes to the selected item type clear selection to prevent mixing different
/// invoice item models in a single return payload.
/// </summary>
public partial class InventoryStockSelection
{
    [Parameter] public ItemType ItemType { get; set; } = ItemType.Product;
    [Parameter] public ItemStatus ItemStatus { get; set; } = ItemStatus.Available;
    [Parameter] public ItemType[] SelectableTypes { get; set; } = Enum.GetValues<ItemType>();
    [Parameter] public bool ShowItemStatus { get; set; } = true;
    [Parameter] public string? SearchQuery { get; set; }
    [Parameter] public Guid? InventoryEntryId { get; set; }
    [Parameter] public Guid? InventoryExitId { get; set; }
    [Parameter] public EventCallback<HashSet<InventoryStockVm>?> SelectedItemsChanged { get; set; }

    private List<InventoryStockVm> _items = [];
    private readonly Dictionary<string, InventoryStockVm> _selected = new();
    private List<ProductCategoryVm> _categories = [];
    private DateRange _dateRange = new();
    private Guid? _categoryId;
    private string? _search;
    private string _sort = "CurrentQuantity";
    private ItemType _type;
    private ItemStatus _status;
    private bool _advancedFilters, _initialized, _loading, _loadFailed;
    private int _pageIndex, _pageSize = 10, _totalItems;
    private int PageCount => Math.Max(1, (_totalItems + _pageSize - 1) / _pageSize);

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (_initialized) return;
        _initialized = true;
        _type = SelectableTypes.Contains(ItemType) ? ItemType : SelectableTypes.FirstOrDefault();
        _status = ItemStatus;
        _search = SearchQuery;
        await LoadAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await SendRequestAsync<IProductCategoryService, List<GetProductCategoryResponse>>(
            action: (service, ct) => service.GetListAsync(ct),
            afterSend: data => _categories = data.Select(ProductCategoryVm.CreateFrom).ToList(),
            createScope: true);
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _loadFailed = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        try
        {
            var request = new RequestFilter(_pageIndex * _pageSize, _pageSize, _search, _sort,
                GoldEx.Sdk.Common.Definitions.SortDirection.Descending);
            var inventory = new InventoryFilter(
                _status == ItemStatus.Available ? WarehouseActionType.In : WarehouseActionType.Out,
                _type, _categoryId, _dateRange.Start, _dateRange.End, InventoryEntryId, InventoryExitId);
            await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
                action: (service, ct) => service.GetListAsync(request, inventory, ct),
                afterSend: result =>
                {
                    _items = result.Data.Select(InventoryStockVm.CreateFrom).ToList();
                    _totalItems = result.Total;
                },
                onFailure: () => _loadFailed = true,
                createScope: true, cancelPrevious: true);
            if (!_loadFailed && _pageIndex >= PageCount)
            {
                _pageIndex = PageCount - 1;
                await LoadAsync();
            }
        }
        finally
        {
            _loading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private static string ItemKey(InventoryStockVm item)
    {
        if (item.Product is not null)
            return "p:" + (item.Product.Id?.ToString() ?? item.Product.Barcode ?? string.Empty);
        if (item.Coin is not null)
            return "c:" + (item.Coin.Id?.ToString() ?? item.Coin.Barcode ?? string.Empty);
        return "u:" + item.Currency?.Id.ToString();
    }
    private static string ItemTitle(InventoryStockVm item) =>
        item.Product?.Name ?? item.Coin?.Coin?.Title ?? item.Currency?.Title ?? "کالای بدون نام";
    private static string ItemBarcode(InventoryStockVm item) =>
        item.Product?.Barcode ?? item.Coin?.Barcode ?? string.Empty;
    private static string ItemDescription(InventoryStockVm item) => item.Product is not null
        ? $"{item.Product.ProductType.GetDisplayName()} · عیار {item.Product.Fineness:G29}"
        : item.Coin is not null ? $"سکه · عیار {item.Coin.Fineness:G29}" : "ارز";
    private static string FormatAmount(InventoryStockVm item, decimal amount) =>
        item.Product is not null ? amount.ToWeightFormat(item.Product.GoldUnitType)
        : amount.ToString("N2");

    private async Task ToggleItem(InventoryStockVm item, bool selected)
    {
        var key = ItemKey(item);
        if (selected) _selected[key] = item;
        else _selected.Remove(key);
        await SelectedItemsChanged.InvokeAsync(new HashSet<InventoryStockVm>(_selected.Values));
    }
    private async Task ClearSelection()
    {
        _selected.Clear();
        await SelectedItemsChanged.InvokeAsync(new HashSet<InventoryStockVm>());
    }
    private async Task OnTypeChanged(ItemType value)
    {
        if (_type == value) return;
        _type = value;
        _categoryId = null;
        _sort = "CurrentQuantity";
        if (_status is ItemStatus.Melted && _type is not ItemType.UsedProduct) _status = ItemStatus.Available;
        await ClearSelection();
        await RefreshAsync();
    }
    private async Task OnStatusChanged(ItemStatus value)
    {
        if (_status == value) return;
        _status = value;
        await ClearSelection();
        await RefreshAsync();
    }
    private async Task OnSearch(string? value)
    {
        if (_search == value) return;
        _search = value;
        await RefreshAsync();
    }
    private async Task OnCategoryChanged(Guid? value)
    {
        _categoryId = value;
        await RefreshAsync();
    }
    private async Task OnDateRangeChanged(DateRange value)
    {
        _dateRange = value;
        await RefreshAsync();
    }
    private async Task OnSortChanged(string value)
    {
        _sort = value;
        await RefreshAsync();
    }
    private void ToggleAdvancedFilters() => _advancedFilters = !_advancedFilters;
    private async Task ClearAdvancedFilters()
    {
        _dateRange = new();
        _categoryId = null;
        _sort = "CurrentQuantity";
        await RefreshAsync();
    }
    private async Task RefreshAsync()
    {
        _pageIndex = 0;
        await LoadAsync();
    }
    private async Task ChangePage(int direction)
    {
        var next = _pageIndex + direction;
        if (_loading || next < 0 || next >= PageCount) return;
        _pageIndex = next;
        await LoadAsync();
    }
    private async Task OnPageSizeChanged(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var size) || size is not (10 or 25 or 50)) return;
        _pageSize = size;
        await RefreshAsync();
    }
}