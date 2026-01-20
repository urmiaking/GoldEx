using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryExits.Components;

public partial class InventoryExitList
{
    private MudTable<InventoryExitResponse> _table = default!;

    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? SearchQuery { get; set; }

    private async Task<TableData<InventoryExitResponse>> LoadInventoryExitsAsync(TableState state,
        CancellationToken cancellationToken)
    {
        var result = new TableData<InventoryExitResponse>();

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
                result = new TableData<InventoryExitResponse>
                {
                    TotalItems = response.Total,
                    Items = response.Data
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

    private async Task Delete(InventoryExitResponse context)
    {
        var result = await DialogService.ShowMessageBox("تأیید حذف",
            "با تایید شما تمامی اجناسی که طی این عملیات از انبار خارچ شده اند، به انبار برمیگردند.",
            yesText: "حذف",
            cancelText: "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IInventoryExitService>(
                action: (s, token) => s.RollbackAsync(context.Id, token),
                afterSend: () =>
                {
                    AddSuccessToast("خروج دستی با موفقیت برگردانده شد");
                    _ = RefreshDataAsync();
                    return Task.CompletedTask;
                }
            );
        }
    }
}