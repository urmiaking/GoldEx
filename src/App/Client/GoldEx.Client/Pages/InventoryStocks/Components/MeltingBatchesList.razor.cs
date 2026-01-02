using GoldEx.Client.Pages.InventoryStocks.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.InventoryStocks.Components;

public partial class MeltingBatchesList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? SearchQuery { get; set; }
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private readonly string _jsVersion = new Random().Next(1, 1000).ToString();
    private MudTable<MeltingBatchListVm> _table = default!;
    private MeltingBatchStatus? _status;
    private DateRange _filterDateRange = new();
    
    private Color StatusColor => _status switch
    {
        MeltingBatchStatus.Melting => Color.Error,
        MeltingBatchStatus.SentToLab => Color.Primary,
        MeltingBatchStatus.Completed => Color.Success,
        _ => Color.Info
    };

    private string StatusIcon => _status switch
    {
        MeltingBatchStatus.Melting => Icons.Material.Filled.Whatshot,
        MeltingBatchStatus.SentToLab => Icons.Material.Filled.Science,
        MeltingBatchStatus.Completed => Icons.Material.Filled.Check,
        _ => Icons.Material.Filled.List
    };

    private async Task<TableData<MeltingBatchListVm>> LoadMeltingBatchAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<MeltingBatchListVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, SearchQuery, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        var inventoryFilter = new MeltingBatchFilter(_status, _filterDateRange.Start, _filterDateRange.End);

        await SendRequestAsync<IMeltingBatchService, PagedList<GetMeltingBatchResponse>>(
            action: (s, token) => s.GetListAsync(filter, inventoryFilter, token),
            afterSend: response =>
            {
                var items = response.Data.Select(MeltingBatchListVm.CreateFrom).ToList();

                result = new TableData<MeltingBatchListVm>
                {
                    TotalItems = response.Total,
                    Items = items
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        return result;
    }

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task OnDateRangeChanged(DateRange dateRange)
    {
        _filterDateRange = dateRange;
        await RefreshAsync();
    }

    private async Task OnSearch(string text)
    {
        SearchQuery = text;

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);

        else
            await _table.ReloadServerData();
    }

    private void PageChanged(int i)
    {
        if (i <= 0)
            return;

        _table.NavigateTo(i - 1);
    }

    private async Task OnPrintBarcode(GetMeltingBatchDetailResponse meltingBatchDetail)
    {
        var labelData = new
        {
            text = meltingBatchDetail.Product.Barcode,
            name = meltingBatchDetail.Product.Name,
            weight = "وزن: " + meltingBatchDetail.Weight.ToString("G29") + $"{(meltingBatchDetail.WeightUnitType is GoldUnitType.Gram ? "g" : "m")}",
            wage = "اجرت: " + meltingBatchDetail.Product.WageType switch
            {
                WageType.Fixed => $"{meltingBatchDetail.Product.Wage?.ToCurrencyFormat(meltingBatchDetail.Product.WagePriceUnitTitle)}",
                WageType.Percent => meltingBatchDetail.Product.Wage?.ToString("G29") + "%",
                _ => "ندارد"
            }
        };

        await JsRuntime.InvokeVoidAsync("printBarcode", labelData);
    }

    private async Task SetStatusText(MeltingBatchStatus? status)
    {
        _status = status;
        await RefreshAsync();
    }

    private void ShowDetails(TableRowClickEventArgs<MeltingBatchListVm> args)
    {
        var batch = _table.FilteredItems.FirstOrDefault(b => b.Id == args.Item?.Id);
        if (batch is not null)
        {
            batch.ShowDetails = !batch.ShowDetails;
        }
    }

    private void OnCreateMeltingBatch()
    {
        Navigation.NavigateTo(ClientRoutes.InventoryStocks.MeltingBatches.Create);
    }

    private void OnProcessBatch(Guid batchId)
    {
        Navigation.NavigateTo(ClientRoutes.InventoryStocks.MeltingBatches.Set.FormatRoute(new { id = batchId }));
    }
}