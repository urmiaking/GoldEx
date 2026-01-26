using GoldEx.Client.Pages.InventoryExits.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryExits.Components;

public partial class InventoryExitList
{
    private MudTable<InventoryExitListVm> _table = default!;

    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? SearchQuery { get; set; }

    private async Task<TableData<InventoryExitListVm>> LoadInventoryExitsAsync(TableState state,
        CancellationToken cancellationToken)
    {
        var result = new TableData<InventoryExitListVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, SearchQuery, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IInventoryExitService, PagedList<InventoryExitResponse>>(
            action: (s, token) => s.GetListAsync(filter, token),
            afterSend: response =>
            {
                result = new TableData<InventoryExitListVm>
                {
                    TotalItems = response.Total,
                    Items = response.Data.Select(InventoryExitListVm.CreateFrom).ToList()
                };
            },
            createScope: true,
            cancelPrevious: false
        );

        return result;
    }

    private async Task RefreshDataAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private void PageChanged(int i)
    {
        if (i <= 0)
            return;

        _table.NavigateTo(i - 1);
    }

    private void ShowDetails(TableRowClickEventArgs<InventoryExitListVm> args)
    {
        var entryItem = _table.FilteredItems.FirstOrDefault(b => b.Equals(args.Item));
        if (entryItem is not null)
        {
            entryItem.ShowDetails = !entryItem.ShowDetails;
        }
    }

    private string GetShowDetailsIcon(bool opened)
    {
        return opened ? Icons.Material.Filled.ExpandLess : Icons.Material.Filled.ExpandMore;
    }

    private ItemType[] GetItemTypes(InventoryExitListVm context)
    {
        var types = new List<ItemType>();

        if (context.ProductsAmount > 0)
        {
            types.Add(ItemType.Product);
            types.Add(ItemType.MoltenGold);
        }

        if (context.CoinsAmount > 0)
            types.Add(ItemType.Coin);

        if (context.CurrenciesAmount > 0)
            types.Add(ItemType.Currency);

        return types.ToArray();
    }

    private ItemType GetSelectedItemType(InventoryExitListVm context)
    {
        if (context.ProductsAmount > 0)
            return ItemType.Product;

        if (context.CoinsAmount > 0)
            return ItemType.Coin;

        if (context.CurrenciesAmount > 0)
            return ItemType.Currency;

        return ItemType.Product;
    }
}