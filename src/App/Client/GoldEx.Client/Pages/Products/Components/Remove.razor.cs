using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class Remove
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public string ProductName { get; set; } = string.Empty;

    [Parameter]
    public Guid Id { get; set; }

    private bool _processing;

    private async Task OnValidSubmit()
    {
        if (_processing)
            return;

        _processing = true;
        await SendRequestAsync<IProductService>((s, ct) => s.DeleteAsync(Id, ct));
        _processing = false;

        MudDialog.Close(DialogResult.Ok(true));

    }

    private void Cancel() => MudDialog.Cancel();
}