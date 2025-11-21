using GoldEx.Client.Pages.Blogs.ViewModels;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace GoldEx.Client.Pages.Blogs.Components;

public partial class BlogCategoryEditor
{
    [Parameter] public BlogCategoryVm Model { get; set; } = new();
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = null!;

    private void Close() => Dialog.Close();

    private async Task Submit(EditContext context)
    {
        var request = Model.ToRequest();

        await SendRequestAsync<IBlogCategoryService>(
            action: (s, ct) => Model.Id.HasValue ? s.UpdateAsync(Model.Id.Value, request, ct) : s.CreateAsync(request, ct),
            afterSend: () =>
            {
                Dialog.Close(true);
                return Task.CompletedTask;
            });
    }
}