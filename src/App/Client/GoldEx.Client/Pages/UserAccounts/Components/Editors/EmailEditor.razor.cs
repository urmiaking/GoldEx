using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts.Components.Editors;

public partial class EmailEditor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public string? Email { get; set; }

    private MudForm _form = default!;

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        var request = new UpdateUserEmailRequest(Email);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.UpdateUserEmailAsync(request, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }
}