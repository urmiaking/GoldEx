using GoldEx.Client.Pages.InventoryEntry.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryEntries;
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

    private async Task<TableData<InventoryEntryListVm>> LoadInventoryEntryAsync(TableState state,
        CancellationToken cancellationToken)
    {
        var result = new TableData<InventoryEntryListVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, null, state.SortLabel,
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
        var result = await DialogService.ShowMessageBox("تأیید حذف",
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
}