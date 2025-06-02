using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Categories;

public partial class Remove
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public string CategoryName { get; set; } = string.Empty;
    [Parameter] public Guid Id { get; set; }

    private bool _processing;

    private async Task OnValidSubmit()
    {
        if (_processing)
            return;

        _processing = true;

        await SendRequestAsync<IProductCategoryService>(
            action: (s, ct) => s.DeleteAsync(Id, ct)
        );

        MudDialog.Close(DialogResult.Ok(true));
        _processing = false;
    }

    private void Cancel() => MudDialog.Cancel();
}