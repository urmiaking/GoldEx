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

    private async Task OnValidSubmit()
    {
        if (IsBusy)
            return;

        var result = await SendRequestAsync<IProductService>((s, ct) => s.DeleteAsync(Id, ct));

        if (result == false)
            return;

        MudDialog.Close(DialogResult.Ok(true));

    }

    private void Cancel() => MudDialog.Cancel();
}