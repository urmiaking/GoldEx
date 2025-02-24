using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Products.Components;

public partial class Create
{
    private readonly ProductVm _model = ProductVm.CreateDefaultInstance();
    private bool _processing;

    [CascadingParameter] 
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private IProductClientService ProductService => GetRequiredService<IProductClientService>();

    protected override void OnInitialized()
    {
        GenerateBarcode();

        base.OnInitialized();
    }

    private async Task OnValidSubmit()
    {
        try
        {
            if (_processing)
                return;

            SetBusy();
            CancelToken();
            _processing = true;
            
            await ProductService.CreateAsync(ProductVm.ToCreateRequest(_model));

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

    private void GenerateBarcode()
    {
        _model.Barcode = StringExtensions.GenerateRandomBarcode();
    }

    private void Close() => MudDialog.Cancel();
}