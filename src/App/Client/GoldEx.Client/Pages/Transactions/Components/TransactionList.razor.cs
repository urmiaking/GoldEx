using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class TransactionList
{
    private string? _searchString;
    private GetPriceUnitTitleResponse? _priceUnit;
    private List<GetPriceUnitTitleResponse>? _priceUnits;
    private MudTable<GetTransactionResponse> _table = new();
    private DateRange _filterDateRange = new();

    private readonly TableGroupDefinition<GetTransactionResponse> _groupDefinition = new()
    {
        GroupName = nameof(GetTransactionResponse.GroupId),
        Selector = e => e.GroupId
    };

    [Parameter] public Guid? InvoiceId { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }
    [Parameter] public string? TableTitle { get; set; }
    [Parameter] public string? ContainerClass { get; set; }
    [Parameter] public bool ShowReversed { get; set; }
    [Parameter] public bool Descending { get; set; }

    private TransactionFilter TransactionFilter => new(InvoiceId,
        CustomerId,
        _priceUnit?.Id,
        null,
        _filterDateRange.Start,
        _filterDateRange.End,
        ShowReversed,
        Descending);

    protected override async Task OnInitializedAsync()
    {
        await LoadPriceUnitsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<ITransactionService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetAvailablePriceUnitsAsync(TransactionFilter, ct),
            afterSend: response => { _priceUnits = response; },
            createScope: true);
    }

    private async Task<TableData<GetTransactionResponse>> LoadTransactionsAsync(TableState state, CancellationToken cancellationToken)
    {
        var result = new TableData<GetTransactionResponse>();

        var requestFilter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<ITransactionService, PagedList<GetTransactionResponse>>(
            action: (s, ct) => s.GetListAsync(TransactionFilter, requestFilter, ct),
            afterSend: response =>
            {
                result = new TableData<GetTransactionResponse>
                {
                    TotalItems = response.Total,
                    Items = response.Data
                };
            },
            cancelPrevious: false,
            createScope: true);

        return result;
    }

    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);

        else
            await _table.ReloadServerData();
    }

    private async Task OnDateRangeChanged(DateRange dateRange)
    {
        _filterDateRange = dateRange;
        await RefreshAsync();
    }

    private async Task SetPriceUnit(GetPriceUnitTitleResponse? priceUnit)
    {
        _priceUnit = priceUnit;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }
}