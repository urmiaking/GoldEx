using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Services;
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
        
        bool result;

        if (Id is null)
        {
            var request = PaymentMethodVm.ToCreateRequest(Model);
            result = await SendRequestAsync<IPaymentMethodService>((s, ct) => s.CreateAsync(request, ct));
        }
        else
        {
            var request = PaymentMethodVm.ToUpdateRequest(Model);
            result = await SendRequestAsync<IPaymentMethodService>((s, ct) => s.UpdateAsync(Model.Id, request, ct));
        }

        if (result == false)
            return;

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Close() => MudDialog.Cancel();
}