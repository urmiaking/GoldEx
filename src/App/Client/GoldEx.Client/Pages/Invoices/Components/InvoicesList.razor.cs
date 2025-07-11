using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class InvoicesList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public Guid? CustomerId { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;

    public string? InvoicePaymentStatusIcon => _invoicePaymentStatus switch
    {
        InvoicePaymentStatus.Paid => Icons.Material.Filled.Check,
        InvoicePaymentStatus.HasDebt => Icons.Material.Filled.Pending,
        InvoicePaymentStatus.Overdue => Icons.Material.Filled.MoreTime,
        null => Icons.Material.Filled.ViewHeadline,
        _ => throw new ArgumentOutOfRangeException()
    };

    private InvoicePaymentStatus? _invoicePaymentStatus;
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

        var invoiceFilter = new InvoiceFilter(_invoicePaymentStatus, _filterDateRange.Start, _filterDateRange.End);

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
            }
        );

        return result;
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;
        await _table.ReloadServerData();
    }

    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);
    }

    public void OnCreateInvoice()
    {
        NavigationManager.NavigateTo(ClientRoutes.Invoices.SetInvoice.FormatRoute( new { Id = "" }).AppendQueryString(new { CustomerId }));
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
        NavigationManager.NavigateTo(ClientRoutes.Invoices.ViewInvoice.FormatRoute(new { number = model.InvoiceNumber }));
    }

    private async Task SetStatusFilterText(InvoicePaymentStatus? status)
    {
        _invoicePaymentStatus = status;
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
}