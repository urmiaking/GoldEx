using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryDirectoryFiltersDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public ItemType ItemType { get; set; }
    [Parameter] public ItemStatus ItemStatus { get; set; }
    [Parameter] public ProductCategoryVm? CategoryFilter { get; set; }
    [Parameter] public List<ProductCategoryVm> Categories { get; set; } = [];
    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public string SortLabel { get; set; } = "CurrentQuantity";
    [Parameter] public bool SortDescending { get; set; } = true;

    private ItemType _itemType;
    private ItemStatus _itemStatus;
    private ProductCategoryVm? _categoryFilter;
    private DateRange _dateRange = new();
    private string _sortLabel = "CurrentQuantity";
    private bool _sortDescending = true;

    private string DateRangeLabel => _itemStatus switch
    {
        ItemStatus.Available => "تاریخ ثبت جنس",
        ItemStatus.Sold => "تاریخ فروش جنس",
        ItemStatus.Melted => "تاریخ ذوب جنس",
        _ => "بازه تاریخ"
    };

    protected override void OnParametersSet()
    {
        _itemType = ItemType;
        _itemStatus = ItemStatus;
        _categoryFilter = CategoryFilter;
        _dateRange = new DateRange(DateRange.Start, DateRange.End);
        _sortLabel = SortLabel;
        _sortDescending = SortDescending;
    }

    private void OnItemTypeChanged(ItemType type)
    {
        if (_itemType == type) return;
        _itemType = type;
        _categoryFilter = null;
        _sortLabel = "CurrentQuantity";
        if (type is ItemType.UsedProduct)
        {
            if (_itemStatus == ItemStatus.Sold) _itemStatus = ItemStatus.Available;
        }
        else if (_itemStatus == ItemStatus.Melted)
            _itemStatus = ItemStatus.Available;
    }

    private static string GetItemTypeIcon(ItemType itemType) => itemType switch
    {
        ItemType.Product => Icons.Material.Filled.Diamond,
        ItemType.MoltenGold => Icons.Material.Filled.Whatshot,
        ItemType.UsedProduct => Icons.Material.Filled.Recycling,
        ItemType.Coin => Icons.Material.Filled.MonetizationOn,
        ItemType.Currency => Icons.Material.Filled.AttachMoney,
        _ => Icons.Material.Filled.Inventory
    };

    private void Apply()
    {
        var selectedStatus = _itemType is not ItemType.UsedProduct && _itemStatus == ItemStatus.Melted
            ? ItemStatus.Available : _itemStatus;
        Session?.Close(new InventoryDirectory.MobileFiltersResult(
            _itemType, selectedStatus, _itemType == ItemType.Product ? _categoryFilter : null,
            new DateRange(_dateRange.Start, _dateRange.End), _sortLabel, _sortDescending));
    }

    private void Clear()
    {
        _itemType = ItemType.Product;
        _itemStatus = ItemStatus.Available;
        _categoryFilter = null;
        _dateRange = new DateRange();
        _sortLabel = "CurrentQuantity";
        _sortDescending = true;
        Apply();
    }

    private void Cancel() => Session?.Cancel();
}
