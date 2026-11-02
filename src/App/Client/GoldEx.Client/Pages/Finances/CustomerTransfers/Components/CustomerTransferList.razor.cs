using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Finances.CustomerTransfers.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.CustomerTransfers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.CustomerTransfers.Components;

public partial class CustomerTransferList
{
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public string ContainerClass { get; set; } = string.Empty;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public bool ShowTitle { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }
    [Parameter] public string? SearchQuery { get; set; }

    [Inject] private ICustomerTransferVoucherService VoucherService { get; set; } = default!;
    [Inject] private IPriceUnitService PriceUnitService { get; set; } = default!;
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

    private List<GetCustomerTransferVoucherListResponse> _items = [];
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private Task? _priceUnitsLoad;
    private DateRange _filterDateRange = new();
    private Guid? _priceUnitId;
    private int _pageIndex, _pageSize = 10, _totalItems;
    private string _sortLabel = "TransferDate";
    private bool _sortDescending = true;
    private bool _loading = true, _loadFailed, _initialized;
    private string? _observedSearch;
    private ElementReference _directory, _filterBar;
    private IJSObjectReference? _stickyModule, _stickyTracker;

    private int PageCount => Math.Max(1, (_totalItems + _pageSize - 1) / _pageSize);
    private int ActiveFilterCount => (_priceUnitId.HasValue ? 1 : 0)
        + (_filterDateRange.Start.HasValue || _filterDateRange.End.HasValue ? 1 : 0);
    private string SortIndicator(string label) => _sortLabel != label ? "↕" : _sortDescending ? "↓" : "↑";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await EnsurePriceUnitsAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (!_initialized)
        {
            _initialized = true;
            _observedSearch = SearchQuery;
            await LoadAsync();
        }
        else if (_observedSearch != SearchQuery)
        {
            _observedSearch = SearchQuery;
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

    private Task EnsurePriceUnitsAsync() => _priceUnitsLoad ??= LoadPriceUnitsAsync();
    private async Task LoadPriceUnitsAsync()
    {
        try { _priceUnits = await PriceUnitService.GetTitlesAsync(); }
        catch (Exception ex) { AddErrorToast($"خطا در دریافت واحدها: {ex.Message}"); }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _loadFailed = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        try
        {
            var filter = new RequestFilter(_pageIndex * _pageSize, _pageSize,
                SearchQuery, _sortLabel, _sortDescending
                    ? GoldEx.Sdk.Common.Definitions.SortDirection.Descending
                    : GoldEx.Sdk.Common.Definitions.SortDirection.Ascending);
            var transferFilter = new CustomerTransferVoucherFilter
            {
                FromDate = _filterDateRange.Start.HasValue ? DateOnly.FromDateTime(_filterDateRange.Start.Value) : null,
                ToDate = _filterDateRange.End.HasValue ? DateOnly.FromDateTime(_filterDateRange.End.Value) : null,
                PriceUnitId = _priceUnitId,
                SourceCustomerId = CustomerId
            };
            await SendRequestAsync<ICustomerTransferVoucherService, PagedList<GetCustomerTransferVoucherListResponse>>(
                action: (service, ct) => service.GetListAsync(filter, transferFilter, ct),
                afterSend: response =>
                {
                    _items = response.Data.ToList();
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
    private Task ReloadAsync() => LoadAsync();

    private async Task OnSearch(string? query)
    {
        if (SearchQuery == query) return;
        SearchQuery = query;
        _observedSearch = query;
        if (!CustomerId.HasValue)
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("q", string.IsNullOrWhiteSpace(query) ? null : query), replace: true);
        await RefreshAsync();
    }
    private async Task ChangePage(int delta)
    {
        var next = _pageIndex + delta;
        if (_loading || next < 0 || next >= PageCount) return;
        _pageIndex = next;
        await LoadAsync();
    }
    private async Task ChangePageSize(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var size) || size is not (10 or 25 or 50) || _pageSize == size) return;
        _pageSize = size;
        await RefreshAsync();
    }
    private async Task ChangeSort(string label)
    {
        if (_sortLabel == label) _sortDescending = !_sortDescending;
        else { _sortLabel = label; _sortDescending = true; }
        await RefreshAsync();
    }
    private async Task OnSortChanged(string label)
    {
        if (_sortLabel == label) return;
        _sortLabel = label;
        await RefreshAsync();
    }
    private async Task ToggleSortDirection() { _sortDescending = !_sortDescending; await RefreshAsync(); }

    private async Task OpenFilters()
    {
        await EnsurePriceUnitsAsync();
        var args = new Dictionary<string, object>
        {
            [nameof(CustomerTransferFiltersDialog.DateRange)] = _filterDateRange,
            [nameof(CustomerTransferFiltersDialog.PriceUnits)] = _priceUnits,
            [nameof(CustomerTransferFiltersDialog.PriceUnitId)] = _priceUnitId!,
            [nameof(CustomerTransferFiltersDialog.SortLabel)] = _sortLabel,
            [nameof(CustomerTransferFiltersDialog.SortDescending)] = _sortDescending
        };
        var dialog = ResponsiveDialogs.Show<CustomerTransferFiltersDialog>("فیلتر حواله‌ها", args);
        var result = await dialog.Result;
        if (result is { Canceled: false, Data: CustomerTransferFiltersDialog.FilterResult values })
        {
            _filterDateRange = new DateRange(values.DateRange.Start, values.DateRange.End);
            _priceUnitId = values.PriceUnitId;
            _sortLabel = values.SortLabel;
            _sortDescending = values.SortDescending;
            await RefreshAsync();
        }
    }
    private async Task ResetFilters()
    {
        SearchQuery = null;
        _observedSearch = null;
        _priceUnitId = null;
        _filterDateRange = new();
        if (!CustomerId.HasValue)
            Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("q", (string?)null), replace: true);
        await RefreshAsync();
    }

    public async Task OnCreate()
    {
        await EnsurePriceUnitsAsync();
        var model = new CustomerTransferVoucherVm { TransferDate = DateTime.Now };
        await ShowEditor("ثبت حواله جدید بین مشتریان", model);
    }
    private async Task OnEdit(GetCustomerTransferVoucherListResponse item)
    {
        try
        {
            await EnsurePriceUnitsAsync();
            var fullItem = await VoucherService.GetAsync(item.Id);
            var model = new CustomerTransferVoucherVm
            {
                Id = fullItem.Id,
                VoucherNumber = fullItem.VoucherNumber,
                TransferDate = fullItem.TransferDate.ToDateTime(TimeOnly.MinValue),
                PriceUnit = _priceUnits.FirstOrDefault(u => u.Id == fullItem.PriceUnitId),
                Amount = fullItem.Amount,
                ExchangeRate = fullItem.ExchangeRate,
                Description = fullItem.Description,
                SourceInvoiceId = fullItem.SourceInvoiceId,
                DestinationInvoiceId = fullItem.DestinationInvoiceId,
                SourceCustomer = new GoldEx.Client.Pages.Customers.ViewModels.CustomerVm
                    { Id = fullItem.SourceCustomerId, FullName = fullItem.SourceCustomerName },
                DestinationCustomer = new GoldEx.Client.Pages.Customers.ViewModels.CustomerVm
                    { Id = fullItem.DestinationCustomerId, FullName = fullItem.DestinationCustomerName }
            };
            await ShowEditor("ویرایش حواله بین مشتریان", model);
        }
        catch (Exception ex) { AddErrorToast($"خطا در بارگذاری حواله: {ex.Message}"); }
    }
    private async Task ShowEditor(string title, CustomerTransferVoucherVm model)
    {
        var args = new Dictionary<string, object>
        {
            [nameof(CustomerTransferEditor.Model)] = model,
            [nameof(CustomerTransferEditor.PriceUnits)] = _priceUnits
        };
        var dialog = ResponsiveDialogs.Show<CustomerTransferEditor>(title, args, wide: true);
        if (await dialog.Result is { Canceled: false }) await LoadAsync();
    }
    private async Task OnDelete(GetCustomerTransferVoucherListResponse item)
    {
        var args = new Dictionary<string, object>
        {
            [nameof(CustomerTransferDeleteDialog.VoucherNumber)] = item.VoucherNumber.ToString(),
            [nameof(CustomerTransferDeleteDialog.AmountDescription)] = item.Amount.ToCurrencyFormat(item.PriceUnitTitle)
        };
        var dialog = ResponsiveDialogs.Show<CustomerTransferDeleteDialog>("حذف حواله", args);
        if (await dialog.Result is not { Canceled: false, Data: true }) return;
        try
        {
            await VoucherService.DeleteAsync(item.Id);
            AddSuccessToast("سند حواله حذف شد.");
            await LoadAsync();
        }
        catch (Exception ex) { AddErrorToast($"خطا در حذف حواله: {ex.Message}"); }
    }
}