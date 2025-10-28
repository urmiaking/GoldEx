using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class TransactionList
{
    private string? _searchString;
    private MudTable<GetTransactionResponse> _table = new();
    private DateRange _filterDateRange = new();

    private TableGroupDefinition<GetTransactionResponse> _groupDefinition = new()
    {
        GroupName = nameof(GetTransactionResponse.GroupId),
        Selector = (e) => e.GroupId
    };

    [Parameter] public Guid? InvoiceId { get; set; }
    [Parameter] public string? Class { get; set; }

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

        var transactionFilter = new TransactionFilter(InvoiceId, _filterDateRange.Start, _filterDateRange.End);

        await SendRequestAsync<ITransactionService, PagedList<GetTransactionResponse>>(
            action: (s, ct) => s.GetListAsync(transactionFilter, requestFilter, ct),
            afterSend: response =>
            {
                result = new TableData<GetTransactionResponse>
                {
                    TotalItems = response.Total,
                    Items = response.Data
                };
            },
            cancelPrevious: true);

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
    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }
}