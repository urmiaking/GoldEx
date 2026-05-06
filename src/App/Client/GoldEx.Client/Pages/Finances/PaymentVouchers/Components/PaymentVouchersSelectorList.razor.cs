using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.PaymentVouchers.Components;

public partial class PaymentVouchersSelectorList
{
    private HashSet<GetPaymentVoucherResponse> _selectedItems = [];
    private IEnumerable<GetPaymentVoucherResponse>? _paymentVouchers = [];

    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public Guid CustomerId { get; set; }
    [Parameter] public List<Guid> SelectedPaymentVouchers { get; set; } = [];

    private void Close() => MudDialog.Cancel();

    protected override async Task OnParametersSetAsync()
    {
        await LoadPaymentVouchersAsync();

        if (SelectedPaymentVouchers.Any())
            _selectedItems = [.._paymentVouchers?.Where(voucher => SelectedPaymentVouchers.Contains(voucher.Id)) ?? []];
        
        await base.OnParametersSetAsync();
    }

    private async Task LoadPaymentVouchersAsync()
    {
        await SendRequestAsync<IPaymentVoucherService, List<GetPaymentVoucherResponse>>(
            action: (service, token) => service.GetPendingListAsync(CustomerId, token),
            afterSend: response =>
            {
                _paymentVouchers = response;
                StateHasChanged();
            }
        );
    }

    private void Submit()
    {
        MudDialog.Close(DialogResult.Ok(_selectedItems));
    }
}