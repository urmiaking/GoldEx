using GoldEx.Shared.DTOs.PersonalAccessTokens;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;

namespace GoldEx.Client.Pages.Settings.Components.PersonalAccessTokens;

public partial class CreateTokenDialog
{
    [Parameter] public CreatePersonalAccessTokenRequest Model { get; set; } = new() { ExpireDays = 90 };
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private async Task Submit()
    {
        if (IsBusy || string.IsNullOrWhiteSpace(Model.Name))
            return;

        await SendRequestAsync<IPersonalAccessTokenService, CreatePersonalAccessTokenResponse>(
            action: (s, ct) => s.CreateAsync(Model, ct),
            afterSend: response =>
            {
                MudDialog.Close(DialogResult.Ok(response));
                return Task.CompletedTask;
            });
    }

    private void Close() => MudDialog.Cancel();
}
