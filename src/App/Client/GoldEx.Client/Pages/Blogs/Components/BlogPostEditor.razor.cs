using GoldEx.Client.Pages.Blogs.ViewModels;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Blogs.Components;

public partial class BlogPostEditor
{
    [Parameter] public BlogPostVm Model { get; set; } = new();
    [Parameter] public EventCallback OnCancelled { get; set; }
    [Parameter] public EventCallback OnSaved { get; set; }

    private async Task Submit(EditContext context)
    {
        await SendRequestAsync<IBlogPostService>(
            (s, ct) => !Model.Id.HasValue
                ? s.CreateAsync(Model.ToRequest(), ct)
                : s.UpdateAsync(Model.Id.Value, Model.ToRequest(), ct),
            afterSend: async () =>
            {
                AddSuccessToast("پست با موفقیت ذخیره شد.");
                await OnSaved.InvokeAsync();
            });
    }

    private async Task GoBack()
    {
        await OnCancelled.InvokeAsync();
    }
}