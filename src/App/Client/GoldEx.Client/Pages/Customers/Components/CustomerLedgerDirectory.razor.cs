using GoldEx.Client.Pages.Customers.Components;
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
    [Parameter] public Guid CustomerId { get; set; }
    private const int PageSize = 10;
    private readonly List<GetTransactionResponse> _items = [];
    private List<GetPriceUnitTitleResponse> _units = [];
    private DateRange _dateRange = new();
    private Guid? _priceUnitId;
    private string? _search;
    private int _page, _total, _loadVersion;
    private bool _loading = true, _loadFailed, _showFilters;
    private int PageCount => Math.Max(1, (_total + PageSize - 1) / PageSize);

    private TransactionFilter Filter => new(
        null, CustomerId, _priceUnitId, null,
        _dateRange.Start, _dateRange.End, false, true);

    private static string TypeClass(GetTransactionResponse item) =>
        item.TransactionType == TransactionType.Credit ? "is-credit" : "is-debit";

    protected override async Task OnInitializedAsync()
    {
        await SendRequestAsync<ITransactionService, List<GetPriceUnitTitleResponse>>(
            (s, ct) => s.GetAvailablePriceUnitsAsync(Filter, ct),
            afterSend: response => _units = response, createScope: true);
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        var version = ++_loadVersion;
        _loading = true;
        _loadFailed = false;
        try
        {
            var request = new RequestFilter(_page * PageSize, PageSize, _search,
                nameof(GetTransactionResponse.PostingDate),
                GoldEx.Sdk.Common.Definitions.SortDirection.Descending);
            await SendRequestAsync<ITransactionService, PagedList<GetTransactionResponse>>(
                (s, ct) => s.GetListAsync(Filter, request, ct),
                afterSend: response =>
                {
                    if (version != _loadVersion) return;
                    _items.Clear();
                    _items.AddRange(response.Data);
                    _total = response.Total;
                },
                onFailure: () =>
                {
                    if (version == _loadVersion) _loadFailed = true;
                }, createScope: true, cancelPrevious: true);
        }
        finally
        {
            if (version == _loadVersion) _loading = false;
        }
    }

    private void ToggleFilters() => _showFilters = !_showFilters;
    private async Task SearchAsync(string? text) { _search = text; _page = 0; await LoadAsync(); }
    private async Task ChangeDatesAsync(DateRange dates) { _dateRange = dates; _page = 0; await LoadAsync(); }
    private async Task ChangeUnitAsync(Guid? id) { _priceUnitId = id; _page = 0; await LoadAsync(); }
    private async Task ClearFiltersAsync()
    {
        _dateRange = new();
        _priceUnitId = null;
        _page = 0;
        await LoadAsync();
    }
    private async Task PreviousAsync() { if (_page <= 0 || _loading) return; _page--; await LoadAsync(); }
    private async Task NextAsync() { if (_page + 1 >= PageCount || _loading) return; _page++; await LoadAsync(); }
}
