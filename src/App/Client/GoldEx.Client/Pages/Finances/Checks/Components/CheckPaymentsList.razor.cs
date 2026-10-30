using GoldEx.Client.Components.Services;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.Checks.Components;

public partial class CheckPaymentsList
{
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public string ContainerClass { get; set; } = string.Empty;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? SearchQuery { get; set; }
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;

    private List<GetCheckPaymentListResponse> _items = [];
    private CheckPaymentStatus? _paymentStatus;
    private string? _searchString, _lastSearch;
    private DateRange _filterDateRange = new();
    private int _pageIndex, _pageSize = 10, _total;
    private string _sortLabel = "DueDate";
    private bool _sortDescending = true, _loading = true, _loadFailed, _initialized;
    private int PageCount => Math.Max(1, (_total + _pageSize - 1) / _pageSize);
    private int ActiveFilterCount => (_paymentStatus.HasValue ? 1 : 0) +
        (_filterDateRange.Start.HasValue || _filterDateRange.End.HasValue ? 1 : 0);

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (!_initialized || _lastSearch != SearchQuery)
        {
            _initialized = true;
            _lastSearch = SearchQuery;
            _searchString = SearchQuery;
            _pageIndex = 0;
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _loading = true;
        _loadFailed = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();
        try
        {
            var request = new RequestFilter(_pageIndex * _pageSize, _pageSize, _searchString, _sortLabel,
                _sortDescending ? GoldEx.Sdk.Common.Definitions.SortDirection.Descending
                                : GoldEx.Sdk.Common.Definitions.SortDirection.Ascending);
            var filters = new CheckPaymentFilter(_paymentStatus, _filterDateRange.Start, _filterDateRange.End);
            await SendRequestAsync<ICheckPaymentService, PagedList<GetCheckPaymentListResponse>>(
                action: (service, ct) => service.GetListAsync(request, filters, ct),
                afterSend: response => { _items = response.Data.ToList(); _total = response.Total; },
                onFailure: () => _loadFailed = true,
                createScope: true, cancelPrevious: true);
            if (!_loadFailed && _pageIndex >= PageCount)
            {
                _pageIndex = PageCount - 1;
                await LoadAsync();
            }
        }
        finally { _loading = false; await InvokeAsync(StateHasChanged); }
    }
    private async Task RefreshAsync() { _pageIndex = 0; await LoadAsync(); }
    private async Task OnSearch(string? value) {
        if (_searchString == value) return;
        _searchString = value; _lastSearch = value;
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("q", string.IsNullOrWhiteSpace(value) ? null : value), replace: true);
        await RefreshAsync();
    }
    private async Task ChangePage(int delta)
    {
        if (_loading || _pageIndex + delta < 0 || _pageIndex + delta >= PageCount) return;
        _pageIndex += delta; await LoadAsync();
    }
    private async Task ChangePageSize(ChangeEventArgs args)
    {
        if (!int.TryParse(args.Value?.ToString(), out var size) || size is not (10 or 25 or 50)) return;
        _pageSize = size; await RefreshAsync();
    }
    private async Task OnSortChanged(string value) { _sortLabel = value; await RefreshAsync(); }
    private async Task ToggleSortDirection() { _sortDescending = !_sortDescending; await RefreshAsync(); }
    private bool IsOverdue(GetCheckPaymentListResponse check) =>
        check.CurrentStatus == CheckPaymentStatus.Pending && check.DueDate.Date < DateTime.Today;
    private string StatusClass(CheckPaymentStatus status) => status switch
    {
        CheckPaymentStatus.Accepted => "is-accepted",
        CheckPaymentStatus.Returned => "is-returned",
        CheckPaymentStatus.Pending => "is-pending",
        _ => ""
    };
    private async Task OpenMobileFilters()
    {
        var args = new Dictionary<string, object>
        {
            [nameof(CheckPaymentFiltersDialog.DateRange)] = _filterDateRange,
            [nameof(CheckPaymentFiltersDialog.PaymentStatus)] = _paymentStatus!,
            [nameof(CheckPaymentFiltersDialog.SortLabel)] = _sortLabel,
            [nameof(CheckPaymentFiltersDialog.SortDescending)] = _sortDescending
        };
        var dialog = ResponsiveDialogs.Show<CheckPaymentFiltersDialog>("فیلتر چک‌ها", args);
        if (await dialog.Result is { Canceled: false, Data: CheckPaymentFiltersDialog.FilterResult result })
        {
            _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
            _paymentStatus = result.PaymentStatus;
            _sortLabel = result.SortLabel;
            _sortDescending = result.SortDescending;
            await RefreshAsync();
        }
    }
    private async Task ResetFilters()
    {
        _searchString = null; _lastSearch = null; _filterDateRange = new(); _paymentStatus = null;
        Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("q", (string?)null), replace: true);
        await RefreshAsync();
    }
    private async Task OnViewCheck(GetCheckPaymentListResponse item)
    {
        var dialog = ResponsiveDialogs.Show<ViewCheckDialog>("جزئیات چک بانکی",
            new Dictionary<string, object> { [nameof(ViewCheckDialog.Check)] = item }, wide: true);
        await dialog.Result;
    }
    private async Task OnAcceptCheck(GetCheckPaymentListResponse item)
    {
        var dialog = ResponsiveDialogs.Show<AcceptCheckDialog>("وصول چک بانکی",
            new Dictionary<string, object> { [nameof(AcceptCheckDialog.Check)] = item }, wide: true);
        if (await dialog.Result is { Canceled: false }) await LoadAsync();
    }
    private async Task OnReturnCheck(GetCheckPaymentListResponse item)
    {
        var dialog = ResponsiveDialogs.Show<ReturnCheckDialog>("برگشت چک بانکی",
            new Dictionary<string, object> { [nameof(ReturnCheckDialog.Check)] = item });
        if (await dialog.Result is { Canceled: false }) await LoadAsync();
    }
}