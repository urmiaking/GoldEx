using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.StoneTypes;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Threading.Tasks;

namespace GoldEx.Client.Pages.Settings.Components.StoneTypes;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public StoneTypeVm Model { get; set; } = new();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private async Task Submit()
    {
        if (IsBusy)
            return;

        if (Id is null)
        {
            var request = StoneTypeVm.ToCreateRequest(Model);
            await SendRequestAsync<IStoneTypeService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            var request = StoneTypeVm.ToUpdateRequest(Model);
            await SendRequestAsync<IStoneTypeService>(
                action: (s, ct) => s.UpdateAsync(Model.Id, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
    }

    private void Close() => MudDialog.Cancel();
}
