using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.LedgerAccounts;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.LedgerAccounts;

public partial class Editor
{
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public LedgerAccountVm Model { get; set; } = new();

    private void Close() => MudDialog.Cancel();

    private async Task OnSubmit()
    {
        if (IsBusy)
            return;

        if (Model.AccountType is null)
        {
            AddErrorToast("نوع حساب نمی تواند خالی باشد");
            return;
        }

        if (Model.Id.HasValue)
        {
            var request = LedgerAccountVm.ToRequest(Model);
            await SendRequestAsync<ILedgerAccountService>(
                action: (s, ct) => s.UpdateAsync(Model.Id.Value, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            var request = LedgerAccountVm.ToRequest(Model);
            await SendRequestAsync<ILedgerAccountService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
    }
}