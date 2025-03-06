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

    private IProductClientService ProductService => GetRequiredService<IProductClientService>();

    private async Task OnValidSubmit()
    {
        try
        {
            SetBusy();
            CancelToken();

            await ProductService.DeleteAsync(Id);

            MudDialog.Close(DialogResult.Ok(true));
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }

    private void Cancel() => MudDialog.Cancel();
}