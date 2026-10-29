using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class InventoryTraceTimeline
{
    [Parameter, EditorRequired] public Guid ItemId { get; set; }
    [Parameter, EditorRequired] public ItemType ItemType { get; set; }

    private List<GetInventoryStockTraceResponse> _items = [];
    private bool _loading, _loadFailed;
    private int _page, _total;
    private const int _pageSize = 6;

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _loadFailed = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        try
        {
            var filter = new RequestFilter(_page * _pageSize, _pageSize, null, null,
                GoldEx.Sdk.Common.Definitions.SortDirection.None);
            await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockTraceResponse>>(
                action: (service, token) => service.GetInventoryStockTracesAsync(ItemId, ItemType, filter, token),
                afterSend: response =>
                {
                    _items = response.Data.ToList();
                    _total = response.Total;
                },
                onFailure: () => _loadFailed = true,
                createScope: true, cancelPrevious: true);
        }
        finally
        {
            _loading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ChangePage(int delta)
    {
        var next = _page + delta;
        if (next < 0 || next * _pageSize >= _total || _loading) return;
        _page = next;
        await LoadAsync();
    }

    private void OpenSource(string sourceUrl) => Navigation.NavigateTo(sourceUrl);

    private static string GetSourceText(string sourceUrl)
    {
        if (sourceUrl.StartsWith(ClientRoutes.Invoices.Index, StringComparison.OrdinalIgnoreCase))
            return "مشاهده فاکتور";
        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.InventoryEntry.Index, StringComparison.OrdinalIgnoreCase))
            return "مشاهده ورود انبار";
        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.MeltingBatches.Index, StringComparison.OrdinalIgnoreCase))
            return "مشاهده درخواست ذوب";
        return "مشاهده منبع";
    }
}
