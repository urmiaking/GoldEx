using GoldEx.Client.Pages.PaymentVouchers.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.PaymentVouchers.Components;

public partial class PaymentVouchersSelectorList
{
    private HashSet<PaymentVoucherListVm> _selectedItems = [];
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public Guid CustomerId { get; set; }
    [Parameter] public List<Guid> SelectedPaymentVouchers { get; set; } = [];

    private void Close() => MudDialog.Cancel();

    private async Task<TableData<PaymentVoucherListVm>> LoadPaymentVouchersAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<PaymentVoucherListVm>();

        var paymentVoucherFilter = new PaymentVoucherFilter(null, null, VoucherStatus.Pending);

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, null, state.SortLabel,
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

    private void Submit()
    {
        MudDialog.Close(DialogResult.Ok(_selectedItems));
    }
}