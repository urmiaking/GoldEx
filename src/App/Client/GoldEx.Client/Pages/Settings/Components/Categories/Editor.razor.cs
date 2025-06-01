using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Categories;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public ProductCategoryVm Model { get; set; } = default!;
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    
    private bool _processing;

    private async Task Submit()
    {
        if (_processing)
            return;

        _processing = true;

        if (Id is null)
        {
            var request = ProductCategoryVm.ToCreateRequest(Model);
            await SendRequestAsync<IProductCategoryService>((s, ct) => s.CreateAsync(request, ct));
        }
        else
        {
            var request = ProductCategoryVm.ToUpdateRequest(Model);
            await SendRequestAsync<IProductCategoryService>((s, ct) => s.UpdateAsync(Model.Id, request, ct));
        }

        _processing = false;

        MudDialog.Close(DialogResult.Ok(true));
    }
    private void Close() => MudDialog.Cancel();
}