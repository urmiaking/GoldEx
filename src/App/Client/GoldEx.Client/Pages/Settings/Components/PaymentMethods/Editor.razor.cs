using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.PaymentMethods;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public PaymentMethodVm Model { get; set; } = new();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private async Task Submit()
    {
        if (IsBusy)
            return;
        
        if (Id is null)
        {
            var request = PaymentMethodVm.ToCreateRequest(Model);
            await SendRequestAsync<IPaymentMethodService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            var request = PaymentMethodVm.ToUpdateRequest(Model);
            await SendRequestAsync<IPaymentMethodService>(
                action: (s, ct) => s.UpdateAsync(Model.Id, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
    }

    private void Close() => MudDialog.Cancel();
}