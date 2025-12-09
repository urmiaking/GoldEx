using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class TransactionList
{
    private MudTable<GetTransactionResponse> _table = new();
    private readonly DateRange _filterDateRange = new();

    private readonly TableGroupDefinition<GetTransactionResponse> _groupDefinition = new()
    {
        GroupName = nameof(GetTransactionResponse.GroupId),
        Selector = (e) => e.GroupId
    };

    [Parameter] public Guid? InvoiceId { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }
    [Parameter] public string? TableTitle { get; set; }
    [Parameter] public string? ContainerClass { get; set; }
    [Parameter] public bool ShowReversed { get; set; }
    [Parameter] public bool Descending { get; set; }

    private async Task<TableData<GetTransactionResponse>> LoadTransactionsAsync(TableState state, CancellationToken cancellationToken)
    {
        var result = new TableData<GetTransactionResponse>();

        var requestFilter = new RequestFilter(state.Page * state.PageSize, state.PageSize, null, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        var transactionFilter = new TransactionFilter(InvoiceId, CustomerId, _filterDateRange.Start, _filterDateRange.End, ShowReversed, Descending);

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

    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);
    }
}