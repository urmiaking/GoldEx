using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryStockTraceList
{
    private MudTable<GetInventoryStockTraceResponse> _table = new();

    [Parameter, EditorRequired] public Guid ItemId { get; set; }
    [Parameter, EditorRequired] public ItemType ItemType { get; set; }

    private async Task<TableData<GetInventoryStockTraceResponse>> LoadItemsAsync(TableState state, CancellationToken cancellationToken)
    {
        var result = new TableData<GetInventoryStockTraceResponse>();

        var requestFilter = new RequestFilter(state.Page * state.PageSize, state.PageSize, null, state.SortLabel,
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
            createScope: true,
            cancelPrevious: true);

        return result;
    }

    private void PageChanged(int i)
    {
        if (i <= 0)
            return;

        _table.NavigateTo(i - 1);
    }

    private void OnViewSource(string sourceUrl)
    {
        Navigation.NavigateTo(sourceUrl);
    }

    private Color GetActionColor(string sourceUrl)
    {
        if (sourceUrl.StartsWith(ClientRoutes.Invoices.Index))
            return Color.Info;

        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.InventoryEntry.Index))
            return Color.Success;

        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.MeltingBatches.Index))
            return Color.Primary;

        throw new NotImplementedException();
    }

    private string GetActionText(string sourceUrl)
    {
        if (sourceUrl.StartsWith(ClientRoutes.Invoices.Index))
            return "مشاهده فاکتور";

        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.InventoryEntry.Index))
            return "مشاهده ورود انبار";

        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.MeltingBatches.Index))
            return "مشاهده درخواست ذوب طلا";

        throw new NotImplementedException();
    }
}