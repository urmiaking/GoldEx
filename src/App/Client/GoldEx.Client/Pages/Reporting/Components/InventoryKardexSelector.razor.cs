using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components;

public partial class InventoryKardexSelector
{
    [CascadingParameter] public IMudDialogInstance Dialog { get; set; } = default!;

    private List<InventoryStockVm>? _selectedItems;

    private void OnSelectedItemsChanged(HashSet<InventoryStockVm>? items)
    {
        if (items is null || !items.Any())
            return;

        _selectedItems = items.ToList();
    }

    private void OnSubmit()
    {
        if (_selectedItems is null || _selectedItems.Count == 0 || _selectedItems.Count > 1)
        {
            AddErrorToast("لطفا یک کالا را انتخاب کنید");
            return;
        }

        Dialog.Close(_selectedItems.First());
    }

    private void Close() => Dialog.Close();
}