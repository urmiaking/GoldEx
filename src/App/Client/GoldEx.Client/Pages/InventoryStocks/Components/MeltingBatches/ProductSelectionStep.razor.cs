using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.InventoryStocks.Components.MeltingBatches;

public partial class ProductSelectionStep
{
    [Parameter] public MeltingBatchVm Model { get; set; } = new();
    [Parameter] public EventCallback<MeltingBatchVm> ModelChanged { get; set; }
    [Parameter] public bool Processing { get; set; }
    [Parameter] public EventCallback OnMeltingProducts { get; set; }

    private async Task OnSelectedItemsChanged(HashSet<InventoryStockVm>? inventoryItems)
    {
        Model.Products = inventoryItems?
            .Where(x => x.Product != null)
            .Select(x => x.Product!).ToList() ?? [];

        (Model.WeightUnitType, Model.TotalWeight) = Model.GetWeightParams(null);

        await ModelChanged.InvokeAsync(Model);
    }
}