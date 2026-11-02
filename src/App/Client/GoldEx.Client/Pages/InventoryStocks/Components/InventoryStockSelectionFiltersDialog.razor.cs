using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryStockSelectionFiltersDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public ItemType ItemType { get; set; }
    [Parameter] public ItemStatus ItemStatus { get; set; }
    [Parameter] public ItemType[] SelectableTypes { get; set; } = [];
    [Parameter] public bool ShowItemStatus { get; set; }
    [Parameter] public List<ProductCategoryVm> Categories { get; set; } = [];
    [Parameter] public Guid? CategoryId { get; set; }
    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public string SortLabel { get; set; } = "CurrentQuantity";

    private ItemType _type;
    private ItemStatus _status;
    private Guid? _categoryId;
    private DateRange _dateRange = new();
    private string _sort = "CurrentQuantity";

    public sealed record FilterResult(ItemType Type, ItemStatus Status, Guid? CategoryId, DateRange DateRange, string SortLabel);
    protected override void OnParametersSet()
    {
        _type = ItemType;
        _status = ItemStatus;
        _categoryId = CategoryId;
        _dateRange = new(DateRange.Start, DateRange.End);
        _sort = SortLabel;
    }
    private void OnTypeChanged(ItemType value)
    {
        if (_type == value) return;
        _type = value;
        _categoryId = null;
        _sort = "CurrentQuantity";
        if (_type is ItemType.UsedProduct && _status is ItemStatus.Sold ||
            _type is not ItemType.UsedProduct && _status is ItemStatus.Melted)
            _status = ItemStatus.Available;
    }
    private void Apply() => Session?.Close(new FilterResult(_type, _status,
        _type == ItemType.Product ? _categoryId : null,
        new(_dateRange.Start, _dateRange.End), _sort));
    private void Reset()
    {
        _type = SelectableTypes.Contains(ItemType) ? ItemType : SelectableTypes.FirstOrDefault();
        _status = ItemStatus.Available;
        _categoryId = null;
        _dateRange = new();
        _sort = "CurrentQuantity";
        Apply();
    }
}