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
    private IProductService ProductService => GetRequiredService<IProductService>();

    private async Task OnValidSubmit()
    {
        try
        {
            if (_processing)
                return;

            SetBusy();
            CancelToken();

            _processing = true;

            await ProductService.DeleteAsync(Id, cancellationToken: CancellationTokenSource.Token);

            MudDialog.Close(DialogResult.Ok(true));
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
            _processing = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();
}