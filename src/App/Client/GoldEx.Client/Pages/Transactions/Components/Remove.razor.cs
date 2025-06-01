using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class Remove
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public long TransactionNumber { get; set; }

    [Parameter] public Guid Id { get; set; }

    private bool _processing;

    private async Task OnValidSubmit()
    {
        if (_processing)
            return;

        _processing = true;
        await SendRequestAsync<ITransactionService>(action: (s, ct) => s.DeleteAsync(Id, ct));
        _processing = false;

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Cancel() => MudDialog.Cancel();
}