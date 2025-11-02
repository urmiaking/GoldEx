using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Shared.Enums;
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
        if (inventoryItems is not null && inventoryItems.Any())
        {
            const decimal mesghalToGramFactor = 4.6083m;

            // Separate by unit type (based on product)
            var gramBased = inventoryItems
                .Where(x => x.Product?.GoldUnitType == GoldUnitType.Gram)
                .ToList();

            var mesghalBased = inventoryItems
                .Where(x => x.Product?.GoldUnitType == GoldUnitType.Mesghal)
                .ToList();

            decimal totalWeight;
            GoldUnitType selectedUnit;

            if (gramBased.Count > mesghalBased.Count)
            {
                selectedUnit = GoldUnitType.Gram;
                var gramWeights = gramBased.Sum(x => x.CurrentAmount);
                var convertedMesghalWeights = mesghalBased.Sum(x => x.CurrentAmount * mesghalToGramFactor);
                totalWeight = gramWeights + convertedMesghalWeights;
            }
            else if (mesghalBased.Count > gramBased.Count)
            {
                selectedUnit = GoldUnitType.Mesghal;
                var mesghalWeights = mesghalBased.Sum(x => x.CurrentAmount);
                var convertedGramWeights = gramBased.Sum(x => x.CurrentAmount / mesghalToGramFactor);
                totalWeight = mesghalWeights + convertedGramWeights;
            }
            else
            {
                // Equal count — default to grams
                selectedUnit = GoldUnitType.Gram;
                var gramWeights = gramBased.Sum(x => x.CurrentAmount);
                var convertedMesghalWeights = mesghalBased.Sum(x => x.CurrentAmount * mesghalToGramFactor);
                totalWeight = gramWeights + convertedMesghalWeights;
            }

            Model.WeightUnitType = selectedUnit;
            Model.TotalWeight = totalWeight;
        }
        else
        {
            Model.TotalWeight = 0;
        }

        await ModelChanged.InvokeAsync(Model);
    }
}