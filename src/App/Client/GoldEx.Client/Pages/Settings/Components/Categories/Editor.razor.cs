using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.ProductCategories;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Settings.Components.Categories;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public ProductCategoryVm Model { get; set; } = new();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (!Id.HasValue) 
            await LoadPrefixAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadPrefixAsync()
    {
        await SendRequestAsync<IProductCategoryService, GetProductCategoryNumberResponse>(
            action: (s, ct) => s.GetLastCodeAsync(ct),
            afterSend: response =>
            {
                Model.PrefixCode = response.Number;
                StateHasChanged();
            });
    }

    private async Task Submit()
    {
        if (IsBusy || Session?.IsBusy == true)
            return;

        Session?.SetBusy(true);
        try
        {
        if (Id is null)
        {
            var request = ProductCategoryVm.ToCreateRequest(Model);
            await SendRequestAsync<IProductCategoryService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    Complete();
                    return Task.CompletedTask;
                });
        }
        else
        {
            var request = ProductCategoryVm.ToUpdateRequest(Model);
            await SendRequestAsync<IProductCategoryService>(
                action: (s, ct) => s.UpdateAsync(Model.Id, request, ct),
                afterSend: () =>
                {
                    Complete();
                    return Task.CompletedTask;
                });
        }
        }
        finally
        {
            Session?.SetBusy(false);
        }
    }

    private void Complete()
    {
        if (Session is not null) Session.Close(true);
        else MudDialog.Close(DialogResult.Ok(true));
    }

    private void Close()
    {
        if (IsBusy) return;
        if (Session is not null) Session.Cancel();
        else MudDialog.Cancel();
    }
}