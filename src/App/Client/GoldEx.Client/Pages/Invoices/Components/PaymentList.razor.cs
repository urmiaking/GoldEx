using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.PaymentMethods;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class PaymentList
{
    [Parameter] public List<InvoicePaymentVm> Items { get; set; } = [];

    private List<GetPaymentMethodResponse> _paymentMethods = [];

    protected override async Task OnParametersSetAsync()
    {
        await LoadPaymentMethodsAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadPaymentMethodsAsync()
    {
        await SendRequestAsync<IPaymentMethodService, List<GetPaymentMethodResponse>>(
            action: (s, ct) => s.GetListAsync(ct),
            afterSend: response =>
            {
                _paymentMethods = response;

                if (!Items.Any())
                {
                    Items.Add(new InvoicePaymentVm
                    {
                        Amount = 0,
                        Note = string.Empty,
                        PaymentDate = DateTime.Now
                    });
                }

                StateHasChanged();
            });
    }

    private void AddItem()
    {
        Items.Add(new InvoicePaymentVm
        {
            Amount = 0,
            Note = string.Empty,
            PaymentDate = DateTime.Now
        });
    }

    private void RemoveItem(InvoicePaymentVm item)
    {
        if (Items.Count > 1)
            Items.Remove(item);
    }
}