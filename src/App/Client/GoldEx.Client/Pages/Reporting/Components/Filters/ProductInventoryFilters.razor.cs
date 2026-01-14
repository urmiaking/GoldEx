using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class ProductInventoryFilters
{
    [Parameter] public ProductInventoryFilterVm Model { get; set; } = default!;

    private List<GetProductCategoryResponse> _productCategories = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadProductCategoriesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadProductCategoriesAsync()
    {
        await SendRequestAsync<IProductCategoryService, List<GetProductCategoryResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response => _productCategories = response);
    }

    private Color GetTypeColor(ItemType item)
    {
        return item switch
        {
            ItemType.Product => Color.Success,
            ItemType.MoltenGold => Color.Primary,
            ItemType.UsedProduct => Color.Error,
            ItemType.Coin => Color.Warning,
            ItemType.Currency => Color.Tertiary,
            _ => Color.Default
        };
    }

    private string GetTypeIcon(ItemType item)
    {
        return item switch
        {
            ItemType.Product => Icons.Material.Filled.Diamond,
            ItemType.MoltenGold => Icons.Material.Filled.Whatshot,
            ItemType.UsedProduct => Icons.Material.Filled.DiscFull,
            ItemType.Coin => Icons.Material.Filled.MonetizationOn,
            ItemType.Currency => Icons.Material.Filled.AttachMoney,
            _ => Icons.Material.Filled.Help
        };
    }

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
        return item switch {
            ItemStatus.Available => Icons.Material.Filled.Warehouse,
            ItemStatus.Sold => Icons.Material.Filled.ShoppingBasket,
            ItemStatus.Melted => Icons.Material.Filled.Whatshot,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }
}