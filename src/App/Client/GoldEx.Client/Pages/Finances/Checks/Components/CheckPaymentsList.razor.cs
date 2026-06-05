using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.Checks.Components;

public partial class CheckPaymentsList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? SearchQuery { get; set; }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(SearchQuery))
        {
            _searchString = SearchQuery;
        }
    }

    public string? PaymentStatusIcon => _paymentStatus switch
    {
        CheckPaymentStatus.Accepted => Icons.Material.Filled.Check,
        CheckPaymentStatus.Pending => Icons.Material.Filled.Pending,
        CheckPaymentStatus.Returned => Icons.Material.Filled.Error,
        null => Icons.Material.Filled.ViewHeadline,
        _ => throw new ArgumentOutOfRangeException()
    };

    public Color PaymentStatusColor => _paymentStatus switch
    {
        CheckPaymentStatus.Accepted => Color.Success,
        CheckPaymentStatus.Pending => Color.Primary,
        CheckPaymentStatus.Returned => Color.Error,
        null => Color.Info,
        _ => throw new ArgumentOutOfRangeException()
    };

    private CheckPaymentStatus? _paymentStatus;
    private string? _searchString;
    private MudTable<GetCheckPaymentListResponse> _table = new();
    private DateRange _filterDateRange = new();

    private async Task<TableData<GetCheckPaymentListResponse>> LoadChecksAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<GetCheckPaymentListResponse>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        var checkFilter = new CheckPaymentFilter(_paymentStatus, _filterDateRange.Start, _filterDateRange.End);

        await SendRequestAsync<ICheckPaymentService, PagedList<GetCheckPaymentListResponse>>(
            action: (service, token) => service.GetListAsync(filter, checkFilter, token),
            afterSend: response =>
            {
                result = new TableData<GetCheckPaymentListResponse>
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

    private async Task SetStatusFilterText(CheckPaymentStatus? status)
    {
        _paymentStatus = status;
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

    private Color GetStatusColor(CheckPaymentStatus status) => status switch
    {
        CheckPaymentStatus.Accepted => Color.Success,
        CheckPaymentStatus.Pending => Color.Primary,
        CheckPaymentStatus.Returned => Color.Error,
        _ => Color.Default
    };

    #region Mobile filters

    public record CheckPaymentsMobileFiltersResult(
        DateRange DateRange,
        CheckPaymentStatus? PaymentStatus
    );

    private async Task OpenMobileFilters()
    {
        var parameters = new DialogParameters<CheckPaymentFiltersDialog>
        {
            { x => x.DateRange, _filterDateRange },
            { x => x.PaymentStatus, _paymentStatus }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        var dialog = await DialogService.ShowAsync<CheckPaymentFiltersDialog>("فیلترها", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false } && result.Data is CheckPaymentsMobileFiltersResult filterResult)
        {
            await ApplyMobileFilters(filterResult);
        }
    }

    private async Task ApplyMobileFilters(CheckPaymentsMobileFiltersResult result)
    {
        _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
        _paymentStatus = result.PaymentStatus;

        await RefreshAsync();
    }

    #endregion

    #region Actions

    private readonly DialogOptions _actionsDialogOptions = new()
    {
        CloseButton = true,
        FullWidth = true,
        MaxWidth = MaxWidth.Small
    };

    private string GetRowClass(GetCheckPaymentListResponse item, int rowNumber)
    {
        if (item.CurrentStatus == CheckPaymentStatus.Pending && item.DueDate.Date < DateTime.Today)
        {
            return "check-overdue-row";
        }
        return string.Empty;
    }

    private async Task OnViewCheck(GetCheckPaymentListResponse item)
    {
        var parameters = new DialogParameters<ViewCheckDialog>
        {
            { x => x.Check, item }
        };

        await DialogService.ShowAsync<ViewCheckDialog>("جزئیات چک بانکی", parameters, new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Medium
        });
    }

    private async Task OnAcceptCheck(GetCheckPaymentListResponse item)
    {
        var parameters = new DialogParameters<AcceptCheckDialog>
        {
            { x => x.Check, item }
        };

        var dialog = await DialogService.ShowAsync<AcceptCheckDialog>("وصول چک بانکی", parameters, _actionsDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            await RefreshAsync();
        }
    }

    private async Task OnReturnCheck(GetCheckPaymentListResponse item)
    {
        var parameters = new DialogParameters<ReturnCheckDialog>
        {
            { x => x.Check, item }
        };

        var dialog = await DialogService.ShowAsync<ReturnCheckDialog>("برگشت چک بانکی", parameters, _actionsDialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            await RefreshAsync();
        }
    }

    #endregion
}
