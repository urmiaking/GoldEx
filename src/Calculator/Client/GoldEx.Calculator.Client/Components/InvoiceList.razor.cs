using GoldEx.Calculator.Client.Services;
using GoldEx.Calculator.Client.ViewModels;
using GoldEx.Client.Components.Calculator.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using System.Text.Json;

namespace GoldEx.Calculator.Client.Components;

public partial class InvoiceList
{
    private bool _isLoggedIn;
    private List<QuickInvoice>? _invoices;
    private MudTable<QuickInvoice> _table = default!;
    private string? _searchString;

    [Inject] private QuickInvoiceStore InvoiceStore { get; set; } = default!;
    [Inject] private IJSRuntime JsRuntime { get; set; } = default!;
    [Parameter] public string? Class { get; set; }

    protected override async Task OnInitializedAsync()
    {
        _isLoggedIn = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User.Identity?.IsAuthenticated == true;

        if (_isLoggedIn) 
            await LoadInvoicesAsync();

        await base.OnInitializedAsync();
    }

    private async Task LoadInvoicesAsync()
    {
        _invoices = await InvoiceStore.GetAllAsync();
    }

    private async Task<TableData<QuickInvoice>> GetInvoiceData(TableState state, CancellationToken cancellationToken)
    {
        if (_invoices is null)
            await LoadInvoicesAsync();

        IEnumerable<QuickInvoice> data = _invoices ?? [];

        // SEARCH (customer name, phone, invoice number)
        if (!string.IsNullOrWhiteSpace(_searchString))
        {
            var search = _searchString.Trim();

            data = data.Where(x =>
                x.InvoiceNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (x.CustomerName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (x.CustomerPhone?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // SORTING
        data = state.SortLabel switch
        {
            nameof(QuickInvoice.InvoiceNumber) =>
                state.SortDirection == SortDirection.Ascending
                    ? data.OrderBy(x => x.InvoiceNumber)
                    : data.OrderByDescending(x => x.InvoiceNumber),

            nameof(QuickInvoice.CustomerName) =>
                state.SortDirection == SortDirection.Ascending
                    ? data.OrderBy(x => x.CustomerName)
                    : data.OrderByDescending(x => x.CustomerName),

            nameof(QuickInvoice.CustomerPhone) =>
                state.SortDirection == SortDirection.Ascending
                    ? data.OrderBy(x => x.CustomerPhone)
                    : data.OrderByDescending(x => x.CustomerPhone),

            nameof(QuickInvoice.DateTime) =>
                state.SortDirection == SortDirection.Ascending
                    ? data.OrderBy(x => x.DateTime)
                    : data.OrderByDescending(x => x.DateTime),

            _ => data.OrderByDescending(x => x.DateTime)
        };

        // PAGING
        var quickInvoices = data.ToList();
        var total = quickInvoices.Count;

        var pageData = quickInvoices
            .Skip(state.Page * state.PageSize)
            .Take(state.PageSize)
            .ToList();

        return new TableData<QuickInvoice>
        {
            Items = pageData,
            TotalItems = total
        };
    }

    private async Task DeleteInvoice(string invoiceNumber)
    {
        var result = await DialogService.ShowMessageBoxAsync("حذف فاکتور",
            $"آیا برای حذف فاکتور شماره {invoiceNumber} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await InvoiceStore.RemoveAsync(invoiceNumber);
            await LoadInvoicesAsync();
            await _table.ReloadServerData();

            AddSuccessToast("فاکتور با موفقیت حذف شد");
        }
    }

    private async Task RePrintInvoice(QuickInvoice invoice)
    {
        var payload = invoice.ToPayload();

        var json = JsonSerializer.Serialize(payload, QuickInvoicePayload.JsonOptions);
        await JsRuntime.InvokeVoidAsync("quickInvoice.printFromPayload", json);
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
}