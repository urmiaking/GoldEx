using GoldEx.Client.Components.Services;
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
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public string ContainerClass { get; set; } = string.Empty;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? SearchQuery { get; set; }
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;

    private List<MeltingBatchListVm> _items = [];
    private MeltingBatchStatus? _status;
    private DateRange _filterDateRange = new();
    private string? _search, _observedSearch;
    private string _sortLabel = "BatchNumber";
    private bool _sortDescending = true;
    private bool _loading = true, _loadFailed, _initialized;
    private int _pageIndex, _pageSize = 10, _totalItems;
    private ElementReference _directory, _filterBar;
    private IJSObjectReference? _stickyModule, _stickyTracker;

    private int PageCount => Math.Max(1, (_totalItems + _pageSize - 1) / _pageSize);
    private int ActiveFilterCount => (_status.HasValue ? 1 : 0) +
        (_filterDateRange.Start.HasValue || _filterDateRange.End.HasValue ? 1 : 0);

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (!_initialized || _observedSearch != SearchQuery)
        {
            _initialized = true;
            _observedSearch = SearchQuery;
            _search = SearchQuery;
            _pageIndex = 0;
            await LoadAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;
        try
        {
            _stickyModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/invoice-sticky-header.js");
            _stickyTracker = await _stickyModule.InvokeAsync<IJSObjectReference>("trackFilterHeight", _directory, _filterBar);
        }
        catch (JSDisconnectedException) { }
    }
    public override async ValueTask DisposeAsync()
    {
        try
        {
            if (_stickyTracker is not null) await _stickyTracker.DisposeAsync();
            if (_stickyModule is not null) await _stickyModule.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        finally { await base.DisposeAsync(); }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _loadFailed = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        try
        {
            var filter = new RequestFilter(_pageIndex * _pageSize, _pageSize, _search, _sortLabel,
                _sortDescending ? GoldEx.Sdk.Common.Definitions.SortDirection.Descending
                                : GoldEx.Sdk.Common.Definitions.SortDirection.Ascending);
            var batchFilter = new MeltingBatchFilter(_status, _filterDateRange.Start, _filterDateRange.End);
            await SendRequestAsync<IMeltingBatchService, PagedList<GetMeltingBatchResponse>>(
                action: (s, ct) => s.GetListAsync(filter, batchFilter, ct),
                afterSend: response =>
                {
                    _items = response.Data.Select(MeltingBatchListVm.CreateFrom).ToList();
                    _totalItems = response.Total;
                },
                onFailure: () => _loadFailed = true,
                createScope: true, cancelPrevious: true);
            if (!_loadFailed && _pageIndex >= PageCount)
            {
                _pageIndex = PageCount - 1;
                await LoadAsync();
                return;
            }
        }
        finally { _loading = false; await InvokeAsync(StateHasChanged); }
    }
    private async Task RefreshAsync() { _pageIndex = 0; await LoadAsync(); }
    private async Task OnSearch(string? value)
    {
        if (_search == value) return;
        _search = value;
        _observedSearch = value;
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("q", string.IsNullOrWhiteSpace(value) ? null : value), replace: true);
        await RefreshAsync();
    }
    private async Task ChangePage(int delta)
    {
        if (_loading || _pageIndex + delta < 0 || _pageIndex + delta >= PageCount) return;
        _pageIndex += delta;
        await LoadAsync();
    }
    private async Task ChangePageSize(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var size) || size is not (10 or 25 or 50) || _pageSize == size) return;
        _pageSize = size;
        await RefreshAsync();
    }
    private async Task OnSortChanged(string label)
    {
        if (_sortLabel == label) return;
        _sortLabel = label;
        await RefreshAsync();
    }
    private async Task ToggleSortDirection()
    {
        _sortDescending = !_sortDescending;
        await RefreshAsync();
    }
    private async Task OpenFilters()
    {
        var dialog = ResponsiveDialogs.Show<MeltingBatchFiltersDialog>("فیلتر درخواست‌های ذوب",
            new Dictionary<string, object>
            {
                [nameof(MeltingBatchFiltersDialog.DateRange)] = _filterDateRange,
                [nameof(MeltingBatchFiltersDialog.Status)] = _status!,
                [nameof(MeltingBatchFiltersDialog.SortLabel)] = _sortLabel,
                [nameof(MeltingBatchFiltersDialog.SortDescending)] = _sortDescending
            });
        if (await dialog.Result is { Canceled: false, Data: MeltingBatchFiltersDialog.FilterResult result })
        {
            _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
            _status = result.Status;
            _sortLabel = result.SortLabel;
            _sortDescending = result.SortDescending;
            await RefreshAsync();
        }
    }
    private async Task ResetFilters()
    {
        _status = null;
        _search = null;
        _observedSearch = null;
        _filterDateRange = new();
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("q", (string?)null), replace: true);
        await RefreshAsync();
    }
    private void ToggleDetails(MeltingBatchListVm item) => item.ShowDetails = !item.ShowDetails;

    private static string StatusClass(MeltingBatchStatus status) => status switch
    {
        MeltingBatchStatus.Melting => "is-melting",
        MeltingBatchStatus.SentToLab => "is-lab",
        MeltingBatchStatus.Completed => "is-completed",
        _ => string.Empty
    };
    private static string ActionLabel(MeltingBatchStatus status) => status switch
    {
        MeltingBatchStatus.Melting => "ارسال به آزمایشگاه",
        MeltingBatchStatus.SentToLab => "ثبت آبشده نهایی",
        _ => "جزئیات"
    };

    private async Task OnPrintBarcode(GetMeltingBatchDetailResponse meltingBatchDetail)
    {
        var labelData = new
        {
            text = meltingBatchDetail.Product.Barcode,
            name = meltingBatchDetail.Product.Name,
            weight = "وزن: " + meltingBatchDetail.Weight.ToString("G29") +
                     (meltingBatchDetail.WeightUnitType is GoldUnitType.Gram ? "g" : "m"),
            wage = "اجرت: " + (meltingBatchDetail.Product.WageType switch
            {
                WageType.Fixed => meltingBatchDetail.Product.Wage?.ToCurrencyFormat(meltingBatchDetail.Product.WagePriceUnitTitle),
                WageType.Percent => meltingBatchDetail.Product.Wage?.ToString("G29") + "%",
                _ => "ندارد"
            })
        };
        await JsRuntime.InvokeVoidAsync("printBarcode", labelData);
    }
    private void OnCreateMeltingBatch() =>
        Navigation.NavigateTo(ClientRoutes.InventoryStocks.MeltingBatches.Create);
    private void OnProcessBatch(Guid batchId) =>
        Navigation.NavigateTo(ClientRoutes.InventoryStocks.MeltingBatches.Set.FormatRoute(new { id = batchId }));
}