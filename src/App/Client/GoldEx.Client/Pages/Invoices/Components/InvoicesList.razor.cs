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

    private bool _isFirstLoad = true;
    private int _initialPage = 0;

    public string? InvoiceTypeIcon => _invoiceType switch
    {
        InvoiceType.Sell => Icons.Material.Filled.ArrowDownward,
        InvoiceType.Purchase => Icons.Material.Filled.ArrowUpward,
        null => Icons.Material.Filled.CompareArrows,
        _ => throw new ArgumentOutOfRangeException()
    };

    public Color InvoiceTypeColor => _invoiceType switch
    {
        InvoiceType.Purchase => Color.Success,
        InvoiceType.Sell => Color.Error,
        null => Color.Info,
        _ => throw new ArgumentOutOfRangeException()
    };

    public string? TradeScaleIcon => _tradeScale switch
    {
        TradeScale.Wholesale => Icons.Material.Filled.Apps,
        TradeScale.Retail => Icons.Material.Filled.Square,
        null => Icons.Material.Filled.ViewHeadline,
        _ => throw new ArgumentOutOfRangeException()
    };

    public Color TradeScaleColor => _tradeScale switch
    {
        TradeScale.Wholesale => Color.Primary,
        TradeScale.Retail => Color.Success,
        null => Color.Info,
        _ => throw new ArgumentOutOfRangeException()
    };

    public string? InvoicePaymentStatusIcon => _invoicePaymentStatus switch
    {
        InvoicePaymentStatus.Paid => Icons.Material.Filled.Check,
        InvoicePaymentStatus.HasDebt => Icons.Material.Filled.Pending,
        InvoicePaymentStatus.Overdue => Icons.Material.Filled.MoreTime,
        null => Icons.Material.Filled.ViewHeadline,
        _ => throw new ArgumentOutOfRangeException()
    };

    public Color InvoicePaymentStatusColor => _invoicePaymentStatus switch
    {
        InvoicePaymentStatus.Paid => Color.Success,
        InvoicePaymentStatus.HasDebt => Color.Primary,
        InvoicePaymentStatus.Overdue => Color.Error,
        null => Color.Info,
        _ => throw new ArgumentOutOfRangeException()
    };

    private InvoicePaymentStatus? _invoicePaymentStatus;
    private InvoiceType? _invoiceType;
    private TradeScale? _tradeScale;
    private string? _searchString;
    private MudTable<InvoiceListVm> _table = new();
    private DateRange _filterDateRange = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

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

    private async Task<TableData<InvoiceListVm>> LoadInvoicesAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<InvoiceListVm>();

        if (_isFirstLoad && PersistState)
        {
            _isFirstLoad = false;
            state.Page = _initialPage;
        }

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        var invoiceFilter = new InvoiceFilter(_invoicePaymentStatus, _invoiceType, _tradeScale, _filterDateRange.Start, _filterDateRange.End);

        await SendRequestAsync<IInvoiceService, PagedList<GetInvoiceListResponse>>(
            action: (service, token) => service.GetListAsync(filter, invoiceFilter, CustomerId, token),
            afterSend: response =>
            {
                var items = response.Data.Select(InvoiceListVm.CreateFrom).ToList();
                result = new TableData<InvoiceListVm>
                {
                    TotalItems = response.Total,
                    Items = items
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        if (PersistState)
        {
            UpdateUrl(state.Page);
        }

        return result;
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;

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
            await RefreshAsync();
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
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private string GetInvoiceDateTooltipText(InvoiceListVm context)
    {
        return $"تاریخ ایجاد: {context.CreatedAt.ToString(CultureInfo.CurrentUICulture)}";
    }

    private void ShowDetails(TableRowClickEventArgs<InvoiceListVm> args)
    {
        var invoice = _table.FilteredItems.FirstOrDefault(b => b.Id == args.Item?.Id);
        if (invoice is not null)
        {
            invoice.ShowDetails = !invoice.ShowDetails;
        }
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

            bool changed = false;

            if (_searchString != newSearch)
            {
                _searchString = newSearch;
                changed = true;
            }

            if (_invoicePaymentStatus != newStatus)
            {
                _invoicePaymentStatus = newStatus;
                changed = true;
            }

            if (_invoiceType != newType)
            {
                _invoiceType = newType;
                changed = true;
            }

            if (_tradeScale != newScale)
            {
                _tradeScale = newScale;
                changed = true;
            }

            if (_filterDateRange?.Start != newStart || _filterDateRange?.End != newEnd)
            {
                _filterDateRange = new DateRange(newStart, newEnd);
                changed = true;
            }

            if (_isFirstLoad)
            {
                _initialPage = newPage;
            }
            else if (_table != null && _table.CurrentPage != newPage)
            {
                _table.NavigateTo(newPage);
            }
            else if (changed && _table != null)
            {
                if (_table.CurrentPage != 0)
                {
                    _table.NavigateTo(0);
                }
                else
                {
                    await _table.ReloadServerData();
                }
            }
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && PersistState && _initialPage > 0)
        {
            if (_table != null)
            {
#pragma warning disable BL0005
                _table.CurrentPage = _initialPage;
#pragma warning restore BL0005
                StateHasChanged();
            }
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void UpdateUrl(int pageIndex)
    {
        if (!PersistState)
            return;

        var parameters = new Dictionary<string, object?>();

        if (pageIndex > 0)
            parameters.Add("page", pageIndex + 1);

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