using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Components.Filters;

public partial class CoinInventoryFilters
{
    [Parameter] public CoinInventoryFilterVm Model { get; set; } = default!;

    private List<GetCoinResponse> _coins = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadCoinsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCoinsAsync()
    {
        await SendRequestAsync<ICoinService, List<GetCoinResponse>>(
            action: (s, ct) => s.GetListAsync(null, ct),
            afterSend: response => _coins = response);
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
        return item switch
        {
            ItemStatus.Available => Icons.Material.Filled.Warehouse,
            ItemStatus.Sold => Icons.Material.Filled.ShoppingBasket,
            ItemStatus.Melted => Icons.Material.Filled.Whatshot,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
    }
}