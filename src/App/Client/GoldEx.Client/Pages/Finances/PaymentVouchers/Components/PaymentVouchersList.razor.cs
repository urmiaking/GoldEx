using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Finances.PaymentVouchers.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.PaymentVouchers.Components;

public partial class PaymentVouchersList
{
    [Parameter] public string Class { get; set; } = string.Empty;
    [Parameter] public string ContainerClass { get; set; } = string.Empty;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public bool ShowTitle { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }
    [Parameter] public string? SearchQuery { get; set; }
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;

    private List<PaymentVoucherListVm> _items = [];
    private DateRange _filterDateRange = new();
    private PaymentVoucherType? _voucherType;
    private int _pageIndex, _pageSize = 10, _total;
    private string _sortLabel = "PaymentDate";
    private bool _sortDescending = true, _loading = true, _loadFailed, _initialized;
    private string? _lastSearch;
    private int PageCount => Math.Max(1, (_total + _pageSize - 1) / _pageSize);
    private int ActiveFilterCount => (_voucherType.HasValue ? 1 : 0)
        + (_filterDateRange.Start.HasValue || _filterDateRange.End.HasValue ? 1 : 0);

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        if (!_initialized || _lastSearch != SearchQuery)
        {
            _initialized = true;
            _lastSearch = SearchQuery;
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
            var request = new RequestFilter(_pageIndex * _pageSize, _pageSize, SearchQuery, _sortLabel,
                _sortDescending ? GoldEx.Sdk.Common.Definitions.SortDirection.Descending
                                : GoldEx.Sdk.Common.Definitions.SortDirection.Ascending);
            var filters = new PaymentVoucherFilter(_filterDateRange.Start, _filterDateRange.End, _voucherType);
            await SendRequestAsync<IPaymentVoucherService, PagedList<GetPaymentVoucherListResponse>>(
                action: (svc, ct) => svc.GetListAsync(request, filters, CustomerId, ct),
                afterSend: response => { _items = response.Data.Select(PaymentVoucherListVm.CreateFrom).ToList(); _total = response.Total; },
                onFailure: () => _loadFailed = true, createScope: true, cancelPrevious: true);
            if (!_loadFailed && _pageIndex >= PageCount)
            {
                _pageIndex = PageCount - 1;
                await LoadAsync();
            }
        }
        finally { _loading = false; await InvokeAsync(StateHasChanged); }
    }

    private async Task RefreshAsync() { _pageIndex = 0; await LoadAsync(); }
    private async Task OnSearch(string? text)
    {
        if (SearchQuery == text) return;
        SearchQuery = text;
        _lastSearch = text;
        if (!CustomerId.HasValue) Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("q", string.IsNullOrWhiteSpace(text) ? null : text), replace: true);
        await RefreshAsync();
    }
    private async Task ChangePage(int delta)
    {
        if (_loading || _pageIndex + delta < 0 || _pageIndex + delta >= PageCount) return;
        _pageIndex += delta; await LoadAsync();
    }
    private async Task ChangePageSize(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var size) || size is not (10 or 25 or 50)) return;
        _pageSize = size; await RefreshAsync();
    }
    private async Task OnSortChanged(string label) { _sortLabel = label; await RefreshAsync(); }
    private async Task ToggleSortDirection() { _sortDescending = !_sortDescending; await RefreshAsync(); }
    private async Task OpenFilters()
    {
        var args = new Dictionary<string, object>
        {
            [nameof(PaymentVoucherFiltersDialog.DateRange)] = _filterDateRange,
            [nameof(PaymentVoucherFiltersDialog.VoucherType)] = _voucherType!,
            [nameof(PaymentVoucherFiltersDialog.SortLabel)] = _sortLabel,
            [nameof(PaymentVoucherFiltersDialog.SortDescending)] = _sortDescending
        };
        var dialog = ResponsiveDialogs.Show<PaymentVoucherFiltersDialog>("فیلتر اسناد پرداخت", args);
        if (await dialog.Result is { Canceled: false, Data: PaymentVoucherFiltersDialog.FilterResult result })
        {
            _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
            _voucherType = result.VoucherType;
            _sortLabel = result.SortLabel;
            _sortDescending = result.SortDescending;
            await RefreshAsync();
        }
    }
    private async Task ResetFilters()
    {
        SearchQuery = null; _lastSearch = null; _filterDateRange = new(); _voucherType = null;
        if (!CustomerId.HasValue) Navigation.NavigateTo(Navigation.GetUriWithQueryParameter("q", (string?)null), replace: true);
        await RefreshAsync();
    }
    public async Task OnCreate()
    {
        var dialog = ResponsiveDialogs.Show<Editor>("افزودن سند پرداخت جدید", wide: true);
        if (await dialog.Result is { Canceled: false })
        {
            AddSuccessToast("سند جدید با موفقیت افزوده شد.");
            await LoadAsync();
        }
    }
    private async Task OnEdit(PaymentVoucherListVm item)
    {
        var dialog = ResponsiveDialogs.Show<Editor>("ویرایش سند پرداخت",
            new Dictionary<string, object> { [nameof(Editor.Id)] = (Guid?)item.Id }, wide: true);
        if (await dialog.Result is { Canceled: false })
        {
            AddSuccessToast("سند پرداخت با موفقیت ویرایش شد.");
            await LoadAsync();
        }
    }
    private async Task OnRemove(PaymentVoucherListVm item)
    {
        var dialog = ResponsiveDialogs.Show<PaymentVoucherDeleteDialog>("حذف سند پرداخت",
            new Dictionary<string, object> {
                [nameof(PaymentVoucherDeleteDialog.VoucherNumber)] = item.VoucherNumber.ToString(),
                [nameof(PaymentVoucherDeleteDialog.AmountText)] = item.Amount.ToCurrencyFormat(item.PriceUnit)
            });
        if (await dialog.Result is not { Canceled: false, Data: true }) return;
        await SendRequestAsync<IPaymentVoucherService>(
            action: (svc, ct) => svc.DeleteAsync(item.Id, ct),
            afterSend: LoadAsync);
    }
    private void OnViewInvoice(PaymentVoucherListVm item) =>
        Navigation.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { id = item.InvoiceId }));
}