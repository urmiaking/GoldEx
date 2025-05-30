using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Categories;

public partial class Create
{
    private readonly ProductCategoryVm _model = new();
    private bool _processing;

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private IProductCategoryService CategoryService => GetRequiredService<IProductCategoryService>();

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        try
        {
            if (_processing)
                return;

            SetBusy();
            CancelToken();
            _processing = true;

            await CategoryService.CreateAsync(ProductCategoryVm.ToCreateRequest(_model));

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
}