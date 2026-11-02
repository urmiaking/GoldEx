using GoldEx.Client.Components.Services;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.PaymentVouchers.Components;

public partial class PaymentVouchersSelectorList
{
    private HashSet<GetPaymentVoucherResponse> _selectedItems = [];
    private IEnumerable<GetPaymentVoucherResponse>? _paymentVouchers;

    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public Guid CustomerId { get; set; }
    [Parameter] public List<Guid> SelectedPaymentVouchers { get; set; } = [];

    private void Close() { if (Session is not null) Session.Cancel(); else MudDialog.Cancel(); }

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

    private void ToggleItem(GetPaymentVoucherResponse item, bool selected)
    {
        if (selected) _selectedItems.Add(item);
        else _selectedItems.Remove(item);
    }

    private void Submit()
    {
        if (Session is not null) Session.Close(_selectedItems);
        else MudDialog.Close(DialogResult.Ok(_selectedItems));
    }
}