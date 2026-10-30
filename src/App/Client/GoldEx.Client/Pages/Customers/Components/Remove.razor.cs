using GoldEx.Client.Components.Services;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class Remove
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public string CustomerName { get; set; } = string.Empty;
    [Parameter] public Guid Id { get; set; }

    private async Task OnValidSubmit()
    {
        if (IsBusy)
            return;

        await SendRequestAsync<ICustomerService>(
            action: (s, ct) => s.DeleteAsync(Id, ct),
            afterSend: () =>
            {
                if (Session is not null) Session.Close(true);
                else MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private void Cancel() { if (IsBusy) return; if (Session is not null) Session.Cancel(); else MudDialog.Cancel(); }
}