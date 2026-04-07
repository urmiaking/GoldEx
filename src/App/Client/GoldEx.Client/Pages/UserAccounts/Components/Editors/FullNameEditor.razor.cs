using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts.Components.Editors;

public partial class FullNameEditor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter, EditorRequired] public string FullName { get; set; }

    private MudForm _form = default!;

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        await _form.Validate();

        if (!_form.IsValid)
            return;

        var request = new UpdateUserFullNameRequest(FullName);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.UpdateUserFullNameAsync(request, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }
}