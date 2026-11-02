using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomerLedgerDirectory
{
    [Parameter, EditorRequired] public Guid CustomerId { get; set; }

    // Match the invoice accounting/inventory timeline: fetch all events in
    // bounded batches, with no manual pagination or artificial record limit.
    private const int TimelineBatchSize = 100;
    private List<GetTransactionResponse> _items = [];
    private List<GetPriceUnitTitleResponse> _units = [];
    private DateRange _dateRange = new();
    private Guid? _priceUnitId;
    private string? _search;
    private int _total, _loadVersion;
    private bool _loading = true, _loadFailed, _showFilters;

    private TransactionFilter Filter => new(
        null, CustomerId, _priceUnitId, null,
        _dateRange.Start, _dateRange.End, false, true);

    private static string TypeClass(GetTransactionResponse item) =>
        item.TransactionType == TransactionType.Credit ? "is-credit" : "is-debit";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await SendRequestAsync<ITransactionService, List<GetPriceUnitTitleResponse>>(
            (service, ct) => service.GetAvailablePriceUnitsAsync(Filter, ct),
            afterSend: response => _units = response, createScope: true);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var version = ++_loadVersion;
        var filter = Filter;
        var search = _search;
        var results = new List<GetTransactionResponse>();
        var total = 0;
        var offset = 0;
        _loading = true;
        _loadFailed = false;
        await InvokeAsync(StateHasChanged);
        try
        {
            do
            {
                List<GetTransactionResponse>? batch = null;
                var failed = false;
                await SendRequestAsync<ITransactionService, PagedList<GetTransactionResponse>>(
                    (service, token) => service.GetListAsync(
                        filter,
                        new RequestFilter(offset, TimelineBatchSize, search,
                            nameof(GetTransactionResponse.PostingDate),
                            GoldEx.Sdk.Common.Definitions.SortDirection.Descending),
                        token),
                    afterSend: response =>
                    {
                        batch = response.Data.ToList();
                        total = response.Total;
                    },
                    onFailure: () => failed = true,
                    cancelPrevious: true,
                    createScope: true);

                if (version != _loadVersion || IsDisposed) return;

                // Do not silently display a partial customer ledger if a later
                // batch failed or the service stopped returning records.
                if (failed || batch is null || (batch.Count == 0 && offset < total))
                {
                    _loadFailed = true;
                    return;
                }
                results.AddRange(batch);
                offset += batch.Count;
            } while (offset < total);

            if (version != _loadVersion || IsDisposed) return;
            _items = results;
            _total = results.Count;
        }
        finally
        {
            if (version == _loadVersion && !IsDisposed)
            {
                _loading = false;
                await InvokeAsync(StateHasChanged);
            }
        }
    }

    private void ToggleFilters() => _showFilters = !_showFilters;
    private async Task SearchAsync(string? text) { _search = text; await LoadAsync(); }
    private async Task ChangeDatesAsync(DateRange dates) { _dateRange = dates; await LoadAsync(); }
    private async Task ChangeUnitAsync(Guid? id) { _priceUnitId = id; await LoadAsync(); }
    private async Task ClearFiltersAsync()
    {
        _dateRange = new();
        _priceUnitId = null;
        await LoadAsync();
    }
}