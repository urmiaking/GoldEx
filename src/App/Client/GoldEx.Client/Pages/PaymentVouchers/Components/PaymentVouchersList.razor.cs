using GoldEx.Client.Pages.PaymentVouchers.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.PaymentVouchers.Components;

public partial class PaymentVouchersList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public Guid? CustomerId { get; set; }

    public string? VoucherStatusIcon => _voucherStatus switch
    {
        VoucherStatus.Pending => Icons.Material.Filled.Pending,
        VoucherStatus.Applied => Icons.Material.Filled.Check,
        null => Icons.Material.Filled.ViewHeadline,
        _ => throw new ArgumentOutOfRangeException()
    };

    private string? _searchString;
    private DateRange _filterDateRange = new();
    private VoucherStatus? _voucherStatus;

    private MudTable<PaymentVoucherListVm> _table = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    private async Task<TableData<PaymentVoucherListVm>> LoadPaymentVouchersAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<PaymentVoucherListVm>();

        var paymentVoucherFilter = new PaymentVoucherFilter(_filterDateRange.Start, _filterDateRange.End, _voucherStatus);

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        await SendRequestAsync<IPaymentVoucherService, PagedList<GetPaymentVoucherListResponse>>(
            action: (service, token) => service.GetListAsync(filter, paymentVoucherFilter, CustomerId, token),
            afterSend: response =>
            {
                var items = response.Data.Select(PaymentVoucherListVm.CreateFrom).ToList();
                result = new TableData<PaymentVoucherListVm>
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

    private async Task OnDateRangeChanged(DateRange dateRange)
    {
        _filterDateRange = dateRange;
        await RefreshAsync();
    }

    private async Task SetStatusFilterText(VoucherStatus? status)
    {
        _voucherStatus = status;
        await RefreshAsync();
    }

    private async Task OnCreate()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن سند پرداخت جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("سند جدید با موفقیت افزوده شد.");
            await RefreshAsync();
        }
    }

    private Task OnPrint(PaymentVoucherListVm context)
    {
        throw new NotImplementedException();
    }

    private async Task OnEdit(PaymentVoucherListVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Id, model.Id }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش سند پرداخت", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("سند پرداخت با موفقیت ویرایش شد.");
            await RefreshAsync();
        }
    }

    private async Task OnRemove(PaymentVoucherListVm model)
    {
        var result = await DialogService.ShowMessageBox(
            "هشدار",
            $"آیا برای حذف سند پرداخت شماره {model.VoucherNumber} مطمئن هستید؟",
            yesText: "بله", cancelText: "لغو");

        if (result is true)
        {
            await SendRequestAsync<IPaymentVoucherService>(
                action: (s, ct) => s.DeleteAsync(model.Id, ct),
                afterSend: RefreshAsync);
        }
    }
}