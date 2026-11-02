using GoldEx.Client.Components.Services;
using GoldEx.Shared.DTOs.CheckPayments;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.Checks.Components;

public partial class ReturnCheckDialog
{
    [Parameter, EditorRequired] public GetCheckPaymentListResponse Check { get; set; } = default!;
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;

    private string? _description;
    private bool _processing;

    private void Close() { if (Session is not null) Session.Cancel(); else Dialog.Cancel(); }

    private async Task Submit()
    {
        _processing = true;
        StateHasChanged();

        var request = new ReturnCheckPaymentRequest(_description);

        await SendRequestAsync<ICheckPaymentService>(
            action: (s, ct) => s.ReturnAsync(Check.Id, request, ct),
            afterSend: () => { if (Session is not null) Session.Close(true); else Dialog.Close(DialogResult.Ok(true)); return Task.CompletedTask; },
            onFailure: () => {
                _processing = false;
                StateHasChanged();
                return Task.CompletedTask;
            }
        );
    }
}