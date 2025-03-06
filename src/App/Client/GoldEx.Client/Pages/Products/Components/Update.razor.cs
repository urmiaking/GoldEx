using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class Update
{
    [Parameter] public ProductVm Model { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private bool _processing;
    private IProductClientService ProductService => GetRequiredService<IProductClientService>();

    public async Task OnValidSubmit()
    {
        try
        {
            if (_processing)
                return;

            SetBusy();
            CancelToken();

            _processing = true;

            await ProductService.UpdateAsync(Model.Id, ProductVm.ToUpdateRequest(Model), CancellationTokenSource.Token);

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

    private void GenerateBarcode() => Model.Barcode = StringExtensions.GenerateRandomBarcode();

    private void Close() => MudDialog.Cancel();
}