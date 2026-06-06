using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryFiltersDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public ItemType ItemType { get; set; }
    [Parameter] public ItemStatus ItemStatus { get; set; }
    [Parameter] public ProductCategoryVm? CategoryFilter { get; set; }
    [Parameter] public List<ProductCategoryVm> Categories { get; set; } = [];
    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public bool Selectable { get; set; }

    private ItemType _itemType;
    private ItemStatus _itemStatus;
    private ProductCategoryVm? _categoryFilter;
    private DateRange? _dateRange;

    private readonly ItemType[] _selectableTypes = Enum.GetValues<ItemType>();

    private string DateRangeLabel => _itemStatus switch
    {
        ItemStatus.Available => "تاریخ ثبت جنس",
        ItemStatus.Sold => "تاریخ فروش جنس",
        ItemStatus.Melted => "تاریخ ذوب جنس",
        _ => "تاریخ"
    };

    protected override void OnParametersSet()
    {
        _itemType = ItemType;
        _itemStatus = ItemStatus;
        _categoryFilter = CategoryFilter;
        _dateRange = new DateRange(DateRange.Start, DateRange.End);
    }

    private void OnItemTypeChanged(ItemType newType)
    {
        _itemType = newType;

        if (_itemType is not ItemType.UsedProduct && _itemStatus == ItemStatus.Melted)
        {
            _itemStatus = ItemStatus.Available;
        }
    }

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

    private string GetStatusIcon(ItemStatus status) => status switch
    {
        ItemStatus.Available => Icons.Material.Filled.Warehouse,
        ItemStatus.Sold => Icons.Material.Filled.ShoppingBasket,
        ItemStatus.Melted => Icons.Material.Filled.Whatshot,
        _ => Icons.Material.Filled.Help
    };

    private Color GetStatusColor(ItemStatus status) => status switch
    {
        ItemStatus.Available => Color.Success,
        ItemStatus.Sold => Color.Error,
        ItemStatus.Melted => Color.Warning,
        _ => Color.Default
    };

    private void Apply()
    {
        var result = new InventoryStockList.MobileFiltersResult(
            _itemType,
            _itemStatus,
            _itemType is ItemType.Product ? _categoryFilter : null,
            new DateRange(_dateRange?.Start, _dateRange?.End)
        );

        MudDialog.Close(DialogResult.Ok(result));
    }

    private void Clear()
    {
        _itemType = ItemType.Product;
        _itemStatus = ItemStatus.Available;
        _categoryFilter = null;
        _dateRange = new DateRange();
        Apply();
    }

    private void Cancel() => MudDialog.Cancel();
}
