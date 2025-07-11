using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class Remove
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public long TransactionNumber { get; set; }

    [Parameter] public Guid Id { get; set; }

    private async Task OnValidSubmit()
    {
        if (IsBusy)
            return;

        await SendRequestAsync<ITransactionService>(
            action: (s, ct) => s.DeleteAsync(Id, ct), 
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private void Cancel() => MudDialog.Cancel();
}