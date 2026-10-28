using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class InvoicesList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public bool ShowTitle { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public bool PersistState { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "page")] public int? PageQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "search")] public string? SearchQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "status")] public string? StatusQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "type")] public string? TypeQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "scale")] public string? ScaleQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "start")] public string? StartQuery { get; set; }
    [Parameter, SupplyParameterFromQuery(Name = "end")] public string? EndQuery { get; set; }

    private bool _initialized, _loading, _loadFailed;
    private int _pageIndex, _pageSize = 14, _totalItems;
    private string _sortLabel = "InvoiceNumber";
    private Sdk.Common.Definitions.SortDirection _sortDirection = Sdk.Common.Definitions.SortDirection.Descending;
    private List<InvoiceListVm> _invoices = [];

    private InvoicePaymentStatus? _invoicePaymentStatus;
    private InvoiceType? _invoiceType;
    private TradeScale? _tradeScale;
    private string? _searchString;
    private DateRange _filterDateRange = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

    [Parameter, SupplyParameterFromQuery(Name = "size")] public int? SizeQuery { get; set; }

    private int PageCount => Math.Max(1, (_totalItems + _pageSize - 1) / _pageSize);
    private int ActiveFilterCount => (_invoicePaymentStatus.HasValue ? 1 : 0) +
        (_invoiceType.HasValue ? 1 : 0) + (_tradeScale.HasValue ? 1 : 0) +
        (_filterDateRange.Start.HasValue || _filterDateRange.End.HasValue ? 1 : 0);
    private bool CanPrevious => !_loading && _pageIndex > 0;
    private bool CanNext => !_loading && _pageIndex + 1 < PageCount;
    private string FilterButtonLabel => ActiveFilterCount == 0 ? "فیلتر فاکتورها" : $"فیلتر فاکتورها، {ActiveFilterCount} فیلتر فعال";

    private string UnpaidClass(InvoiceListVm invoice)
    {
        if (invoice.PaymentStatus == InvoicePaymentStatus.Paid) return "is-paid";
        if (invoice.PaymentStatus == InvoicePaymentStatus.Overdue) return "is-overdue";
        var color = GetUnpaidAmountColor(invoice.TotalUnpaidAmount, invoice.InvoiceType);
        return color == Color.Success ? "is-credit" : color == Color.Error ? "is-debit" : "";
    }

    private string FormatCompleteUnpaidAmount(decimal totalUnpaidAmount, string? primaryUnit, decimal? totalUnpaidSecondaryAmount, string? secondaryUnit)
    {
        var primaryAmount = Math.Abs(totalUnpaidAmount).ToCurrencyFormat(primaryUnit);
        var secondaryPart = !string.IsNullOrEmpty(secondaryUnit) && totalUnpaidSecondaryAmount.HasValue
            ? $" ({Math.Abs(totalUnpaidSecondaryAmount.Value).ToCurrencyFormat(secondaryUnit)})"
            : string.Empty;

        return $"{primaryAmount}{secondaryPart}";
    }

    private Color GetUnpaidAmountColor(decimal amount, InvoiceType invoiceType)
    {
        return amount switch
        {
            > 0 when invoiceType == InvoiceType.Purchase => Color.Success,
            < 0 when invoiceType == InvoiceType.Purchase => Color.Error,
            > 0 when invoiceType == InvoiceType.Sell => Color.Error,
            < 0 when invoiceType == InvoiceType.Sell => Color.Success,
            _ => Color.Inherit
        };
    }

    private async Task LoadInvoicesAsync()
    {
        _loading = true;
        _loadFailed = false;
        StateHasChanged();

        try
        {
            var filter = new RequestFilter(_pageIndex * _pageSize, _pageSize, _searchString,
                _sortLabel, _sortDirection);
            var invoiceFilter = new InvoiceFilter(_invoicePaymentStatus, _invoiceType, _tradeScale,
                _filterDateRange.Start, _filterDateRange.End);
            await SendRequestAsync<IInvoiceService, PagedList<GetInvoiceListResponse>>(
                action: (service, token) => service.GetListAsync(filter, invoiceFilter, CustomerId, token),
                afterSend: response =>
                {
                    _invoices = response.Data.Select(InvoiceListVm.CreateFrom).ToList();
                    _totalItems = response.Total;
                },
                onFailure: () => { _loadFailed = true; },
                createScope: true,
                cancelPrevious: true);
            if (!_loadFailed && _pageIndex >= PageCount)
            {
                _pageIndex = PageCount - 1;
                await LoadInvoicesAsync();
                return;
            }
            if (!_loadFailed && PersistState) UpdateUrl(_pageIndex);
        }
        finally
        {
            _loading = false;
            StateHasChanged();
        }
    }

    private async Task OnSearch(string text)
    {
        if (_searchString == text) return;
        _searchString = text;
        _pageIndex = 0;
        await LoadInvoicesAsync();
    }

    private string SortIndicator(string label)
    {
        if (_sortLabel != label) return "↕";
        return _sortDirection == Sdk.Common.Definitions.SortDirection.Ascending ? "↑" : "↓";
    }

    private async Task ClearAllSearchAndFilters()
    {
        _searchString = string.Empty;
        _invoiceType = null;
        _tradeScale = null;
        _invoicePaymentStatus = null;
        _filterDateRange = new DateRange();
        await RefreshAsync();
    }
    private async Task ChangePage(int delta)
    {
        var nextPage = _pageIndex + delta;
        if (_loading || nextPage < 0 || nextPage >= PageCount || nextPage == _pageIndex) return;
        _pageIndex = nextPage;
        await LoadInvoicesAsync();
    }

    private async Task ChangePageSize(ChangeEventArgs e)
    {
        if (!int.TryParse(e.Value?.ToString(), out var size) || size is not (10 or 14 or 25 or 50) || size == _pageSize) return;
        _pageSize = size;
        _pageIndex = 0;
        await LoadInvoicesAsync();
    }

    private async Task ChangeSort(string label)
    {
        if (_sortLabel == label)
            _sortDirection = _sortDirection == Sdk.Common.Definitions.SortDirection.Descending
                ? Sdk.Common.Definitions.SortDirection.Ascending
                : Sdk.Common.Definitions.SortDirection.Descending;
        else
            _sortLabel = label;

        _pageIndex = 0;
        await LoadInvoicesAsync();
    }

    private async Task OnSortChanged(string label)
    {
        if (string.IsNullOrEmpty(label) || _sortLabel == label) return;
        _sortLabel = label;
        _pageIndex = 0;
        await LoadInvoicesAsync();
    }

    private async Task ToggleSortDirection()
    {
        _sortDirection = _sortDirection == Sdk.Common.Definitions.SortDirection.Descending
            ? Sdk.Common.Definitions.SortDirection.Ascending
            : Sdk.Common.Definitions.SortDirection.Descending;
        _pageIndex = 0;
        await LoadInvoicesAsync();
    }

    private async Task ClearFilters()
    {
        _invoiceType = null;
        _tradeScale = null;
        _invoicePaymentStatus = null;
        _filterDateRange = new DateRange();
        await RefreshAsync();
    }

    public void OnCreateInvoice(TradeScale tradeScale)
    {
        NavigationManager.NavigateTo(ClientRoutes.Invoices.Create.AppendQueryString(new { CustomerId, TradeScale = tradeScale }));
    }

    private async Task OnRemoveInvoice(InvoiceListVm model)
    {
        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id },
            { x => x.InvoiceNumber, model.InvoiceNumber }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف فاکتور", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("فاکتور با موفقیت حذف شد.");
            await ReloadAsync();
        }
    }

    private void OnEditInvoice(InvoiceListVm model)
    {
        NavigationManager.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute(new { model.Id }));
    }

    private void OnViewInvoice(InvoiceListVm model)
    {
        NavigationManager.NavigateTo(ClientRoutes.Invoices.ViewInvoice.FormatRoute(new
        {
            number = model.InvoiceNumber,
            invoiceType = model.InvoiceType.ToString()
        }));
    }

    private async Task SetStatusFilterText(InvoicePaymentStatus? status)
    {
        _invoicePaymentStatus = status;
        await RefreshAsync();
    }

    private async Task SetInvoiceTypeText(InvoiceType? invoiceType)
    {
        _invoiceType = invoiceType;
        await RefreshAsync();
    }

    private async Task SetTradeScaleText(TradeScale? tradeScale)
    {
        _tradeScale = tradeScale;
        await RefreshAsync();
    }

    private async Task OnDateRangeChanged(DateRange dateRange)
    {
        _filterDateRange = dateRange;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        _pageIndex = 0;
        await LoadInvoicesAsync();
    }

    private Task ReloadAsync() => LoadInvoicesAsync();

    private static DateTime GetInvoicePostingDate(InvoiceListVm context)
    {
        return context.InvoiceDate
            .ToDateTime(TimeOnly.FromTimeSpan(context.CreatedAt.TimeOfDay))
            .AddSeconds(1);
    }

    private string GetInvoiceDateTooltipText(InvoiceListVm context)
    {
        return $"تاریخ ایجاد: {context.CreatedAt.ToString(CultureInfo.CurrentUICulture)}";
    }

    private void ToggleDetails(InvoiceListVm invoice)
    {
        invoice.ShowDetails = !invoice.ShowDetails;
    }

    #region Mobile filters

    public record InvoicesMobileFiltersResult(
            DateRange DateRange,
            InvoiceType? InvoiceType,
            TradeScale? TradeScale,
            InvoicePaymentStatus? PaymentStatus
    );

    private async Task OpenMobileFilters()
    {
        var parameters = new DialogParameters<InvoiceFiltersDialog>
        {
            { x => x.DateRange, _filterDateRange },
            { x => x.InvoiceTypeParam, _invoiceType },
            { x => x.TradeScaleParam, _tradeScale },
            { x => x.PaymentStatus, _invoicePaymentStatus }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        var dialog = await DialogService.ShowAsync<InvoiceFiltersDialog>("فیلترها", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false } && result.Data is InvoicesMobileFiltersResult filterResult)
        {
            await ApplyMobileFilters(filterResult);
        }
    }

    private async Task ApplyMobileFilters(InvoicesMobileFiltersResult result)
    {
        _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
        _invoiceType = result.InvoiceType;
        _tradeScale = result.TradeScale;
        _invoicePaymentStatus = result.PaymentStatus;

        await RefreshAsync();
    }

    #endregion

    private string GetInvoiceDate(InvoiceListVm context)
    {
        var text = context.InvoiceDate.ToShortDateString();

        if (context.DueDate.HasValue) 
            text += $" ({context.DueDate.Value.ToShortDateString()})";

        return text;
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        if (PersistState)
        {
            var newPage = (PageQuery ?? 1) - 1;
            if (newPage < 0) newPage = 0;

            var newSearch = SearchQuery;

            InvoicePaymentStatus? newStatus = null;
            if (!string.IsNullOrWhiteSpace(StatusQuery) && Enum.TryParse<InvoicePaymentStatus>(StatusQuery, true, out var parsedStatus))
            {
                newStatus = parsedStatus;
            }

            InvoiceType? newType = null;
            if (!string.IsNullOrWhiteSpace(TypeQuery) && Enum.TryParse<InvoiceType>(TypeQuery, true, out var parsedType))
            {
                newType = parsedType;
            }

            TradeScale? newScale = null;
            if (!string.IsNullOrWhiteSpace(ScaleQuery) && Enum.TryParse<TradeScale>(ScaleQuery, true, out var parsedScale))
            {
                newScale = parsedScale;
            }

            DateTime? newStart = null;
            if (!string.IsNullOrWhiteSpace(StartQuery) && DateTime.TryParseExact(StartQuery, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedStart))
            {
                newStart = parsedStart;
            }

            DateTime? newEnd = null;
            if (!string.IsNullOrWhiteSpace(EndQuery) && DateTime.TryParseExact(EndQuery, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedEnd))
            {
                newEnd = parsedEnd;
            }

            var newSize = SizeQuery is 10 or 14 or 25 or 50 ? SizeQuery.Value : 14;
            var changed = _searchString != newSearch || _invoicePaymentStatus != newStatus ||
                _invoiceType != newType || _tradeScale != newScale ||
                _filterDateRange.Start != newStart || _filterDateRange.End != newEnd ||
                _pageSize != newSize || _pageIndex != newPage;
            if (!_initialized || changed)
            {
                _searchString = newSearch;
                _invoicePaymentStatus = newStatus;
                _invoiceType = newType;
                _tradeScale = newScale;
                _filterDateRange = new DateRange(newStart, newEnd);
                _pageSize = newSize;
                _pageIndex = newPage;
                if (_initialized) await LoadInvoicesAsync();
            }
        }

        if (!_initialized)
        {
            _initialized = true;
            await LoadInvoicesAsync();
        }
    }

    private void UpdateUrl(int pageIndex)
    {
        if (!PersistState)
            return;

        var parameters = new Dictionary<string, object?>();

        if (pageIndex > 0)
            parameters.Add("page", pageIndex + 1);

        if (_pageSize != 14)
            parameters.Add("size", _pageSize);

        if (!string.IsNullOrWhiteSpace(_searchString))
            parameters.Add("search", _searchString);

        if (_invoicePaymentStatus.HasValue)
            parameters.Add("status", _invoicePaymentStatus.Value.ToString());

        if (_invoiceType.HasValue)
            parameters.Add("type", _invoiceType.Value.ToString());

        if (_tradeScale.HasValue)
            parameters.Add("scale", _tradeScale.Value.ToString());

        if (_filterDateRange?.Start.HasValue == true)
            parameters.Add("start", _filterDateRange.Start.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        if (_filterDateRange?.End.HasValue == true)
            parameters.Add("end", _filterDateRange.End.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

        var baseUrl = NavigationManager.Uri.Split('?')[0];
        var newUrl = baseUrl.AppendQueryString(parameters);

        if (NavigationManager.Uri != newUrl)
        {
            NavigationManager.NavigateTo(newUrl, replace: true);
        }
    }
}