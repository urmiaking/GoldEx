using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryStockTraceList
{
    private string? _searchString;
    private MudTable<GetInventoryStockTraceResponse> _table = new();

    [Parameter, EditorRequired] public Guid ItemId { get; set; }
    [Parameter, EditorRequired] public ItemType ItemType { get; set; }

    private async Task<TableData<GetInventoryStockTraceResponse>> LoadItemsAsync(TableState state, CancellationToken cancellationToken)
    {
        var result = new TableData<GetInventoryStockTraceResponse>();

        var requestFilter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockTraceResponse>>(
            action: (s, ct) => s.GetInventoryStockTracesAsync(ItemId, ItemType, requestFilter, ct),
            afterSend: response =>
            {
                result = new TableData<GetInventoryStockTraceResponse>
                {
                    TotalItems = response.Total,
                    Items = response.Data
                };
            },
            cancelPrevious: true);

        return result;
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;
        await RefreshAsync();
    }

    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);
    }
    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private void OnViewSource(string sourceUrl)
    {
        Navigation.NavigateTo(sourceUrl);
    }
}