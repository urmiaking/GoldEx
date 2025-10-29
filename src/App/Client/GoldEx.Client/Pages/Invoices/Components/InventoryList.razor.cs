using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class InventoryList
{
    private string? _searchString;
    private MudTable<GetInventoryStockItemResponse> _table = new();

    [Parameter, EditorRequired] public Guid InvoiceId { get; set; }

    private async Task<TableData<GetInventoryStockItemResponse>> LoadItemsAsync(TableState state, CancellationToken cancellationToken)
    {
        var result = new TableData<GetInventoryStockItemResponse>();

        var requestFilter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockItemResponse>>(
            action: (s, ct) => s.GetInvoiceInventoryItemsAsync(InvoiceId, requestFilter, ct),
            afterSend: response =>
            {
                result = new TableData<GetInventoryStockItemResponse>
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
}