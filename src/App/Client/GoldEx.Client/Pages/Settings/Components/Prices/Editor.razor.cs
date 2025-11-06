using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Prices;

public partial class Editor
{
    [Parameter, EditorRequired] public PriceSettingDto Price { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly PriceSettingVm _model = new();

    private async Task Submit()
    {
        if (IsBusy)
            return;

        byte[]? uploadedFile = null;

        if (_model.IconFile is not null)
        {
            await using var stream = _model.IconFile.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            uploadedFile = memoryStream.ToArray();
        }

        if (uploadedFile is null)
            return;

        var request = PriceSettingVm.ToRequest(uploadedFile);

        await SendRequestAsync<IPriceService>(
            action: (s, ct) => s.UpdateAsync(Price.Id, request, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private void Close() => MudDialog.Cancel();
}