using GoldEx.Client.Pages.InventoryEntry.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryEntry.Components;

public partial class InventoryEntryList
{
    private MudTable<InventoryEntryListVm> _table = default!;
    private bool _isProcessing;

    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? SearchQuery { get; set; }

    private async Task<TableData<InventoryEntryListVm>> LoadInventoryEntryAsync(TableState state,
        CancellationToken cancellationToken)
    {
        var result = new TableData<InventoryEntryListVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, SearchQuery, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IInventoryEntryService, PagedList<InventoryEntryResponse>>(
            action: (s, token) => s.GetListAsync(filter, token),
            afterSend: response =>
            {
                result = new TableData<InventoryEntryListVm>
                {
                    TotalItems = response.Total,
                    Items = response.Data.Select(InventoryEntryListVm.CreateFrom).ToList()
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

    private async Task DeleteEntry(InventoryEntryListVm context)
    {
        var result = await DialogService.ShowMessageBoxAsync("تأیید حذف",
            "با تایید شما تمامی اجناسی که طی این عملیات به انبار افزوده شده اند، حذف خواهند شد",
            yesText: "حذف",
            cancelText: "انصراف");

        if (result == true)
        {
            _isProcessing = true;
            StateHasChanged(); // Force UI to render the overlay immediately

            await SendRequestAsync<IInventoryEntryService>(
                action: (s, token) => s.RollbackAsync(context.Id, token),
                afterSend: () =>
                {
                    AddSuccessToast("ورود با موفقیت حذف شد");
                    _ = RefreshDataAsync();
                    _isProcessing = false;
                    return Task.CompletedTask;
                },
                onFailure: () =>
                {
                    _isProcessing = false;
                    return Task.CompletedTask;
                }
            );
        }
    }

    private void ShowDetails(TableRowClickEventArgs<InventoryEntryListVm> args)
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

    private ItemType[] GetItemTypes(InventoryEntryListVm context)
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

    private ItemType GetSelectedItemType(InventoryEntryListVm context)
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