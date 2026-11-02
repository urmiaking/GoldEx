using GoldEx.Client.Components.Services;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class Remove
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public string ProductName { get; set; } = string.Empty;
    [Parameter] public Guid Id { get; set; }

    private async Task OnValidSubmit()
    {
        if (IsBusy)
            return;

        await SendRequestAsync<IInventoryStockService>(
            action: (s, ct) => s.DeleteProductAsync(Id, ct),
            afterSend: () =>
            {
                if (Session is not null) Session.Close(true);
                else MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private void Cancel() { if (IsBusy) return; if (Session is not null) Session.Cancel(); else MudDialog.Cancel(); }
}