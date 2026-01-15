using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class CurrencyInventoryFilters
{
    [Parameter] public CurrencyInventoryFilterVm Model { get; set; } = default!;

    private Color GetStatusColor(ItemStatus item)
    {
        return item switch
        {
            ItemStatus.Available => Color.Success,
            ItemStatus.Sold => Color.Error,
            ItemStatus.Melted => Color.Warning,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }

    private string GetStatusIcon(ItemStatus item)
    {
        return item switch
        {
            ItemStatus.Available => Icons.Material.Filled.Warehouse,
            ItemStatus.Sold => Icons.Material.Filled.ShoppingBasket,
            ItemStatus.Melted => Icons.Material.Filled.Whatshot,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }
}