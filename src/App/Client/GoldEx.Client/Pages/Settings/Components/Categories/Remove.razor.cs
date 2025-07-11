using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Categories;

public partial class Remove
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public string CategoryName { get; set; } = string.Empty;
    [Parameter] public Guid Id { get; set; }

    private async Task OnValidSubmit()
    {
        if (IsBusy)
            return;

        await SendRequestAsync<IProductCategoryService>(
            action: (s, ct) => s.DeleteAsync(Id, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private void Cancel() => MudDialog.Cancel();
}