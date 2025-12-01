using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
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

    private async Task<TableData<InvoiceListVm>> LoadInvoicesAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<InvoiceListVm>();

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
            cancelPrevious: true
        );

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
}