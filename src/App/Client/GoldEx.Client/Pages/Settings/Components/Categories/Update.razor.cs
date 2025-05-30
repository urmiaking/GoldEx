using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Categories;

public partial class Update
{
    [Parameter] public ProductCategoryVm Model { get; set; } = default!;
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

            await CategoryService.UpdateAsync(Model.Id, ProductCategoryVm.ToUpdateRequest(Model), CancellationTokenSource.Token);

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