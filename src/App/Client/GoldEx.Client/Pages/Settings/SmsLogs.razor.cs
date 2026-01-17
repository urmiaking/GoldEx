using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.SmsLogs;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings;

public partial class SmsLogs
{
    private MudTable<SmsLogResponse> _table = new();
    private string? _searchString;

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

    private async Task RefreshDataAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task<TableData<SmsLogResponse>> LoadSmsLogsAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<SmsLogResponse>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<ISmsLogService, PagedList<SmsLogResponse>>(
            action: (service, token) => service.GetListAsync(filter, token),
            afterSend: response =>
            {
                result = new TableData<SmsLogResponse>
                {
                    TotalItems = response.Total,
                    Items = response.Data
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        return result;
    }
}