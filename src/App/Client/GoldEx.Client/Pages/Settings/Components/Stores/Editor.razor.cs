using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Stores;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System;
using System.IO;
using System.Threading.Tasks;

namespace GoldEx.Client.Pages.Settings.Components.Stores;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public StoreVm Model { get; set; } = new();
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private string? _logoPreviewUrl;
    private string? _backgroundPreviewUrl;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (Id != null)
        {
            _logoPreviewUrl = Model.LogoUrl;
            _backgroundPreviewUrl = Model.BackgroundImageUrl;
        }
    }

    private async Task OnLogoFileChanged(IBrowserFile file)
    {
        if (file == null) return;
        Model.LogoFile = file;

        try
        {
            await using var stream = file.OpenReadStream(2 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            _logoPreviewUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
        }
        catch (Exception ex)
        {
            AddErrorToast($"خطا در بارگذاری لوگو: {ex.Message}");
        }
    }

    private async Task OnBackgroundFileChanged(IBrowserFile file)
    {
        if (file == null) return;
        Model.BackgroundImageFile = file;

        try
        {
            await using var stream = file.OpenReadStream(5 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            _backgroundPreviewUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(bytes)}";
        }
        catch (Exception ex)
        {
            AddErrorToast($"خطا در بارگذاری پس‌زمینه: {ex.Message}");
        }
    }

    private async Task Submit()
    {
        if (IsBusy)
            return;

        byte[]? logoContent = null;
        string? logoExt = null;
        if (Model.LogoFile != null)
        {
            try
            {
                await using var stream = Model.LogoFile.OpenReadStream(2 * 1024 * 1024);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                logoContent = ms.ToArray();
                logoExt = Path.GetExtension(Model.LogoFile.Name).TrimStart('.');
            }
            catch (Exception ex)
            {
                AddErrorToast($"خطا در پردازش لوگو: {ex.Message}");
                return;
            }
        }

        byte[]? bgContent = null;
        string? bgExt = null;
        if (Model.BackgroundImageFile != null)
        {
            try
            {
                await using var stream = Model.BackgroundImageFile.OpenReadStream(5 * 1024 * 1024);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                bgContent = ms.ToArray();
                bgExt = Path.GetExtension(Model.BackgroundImageFile.Name).TrimStart('.');
            }
            catch (Exception ex)
            {
                AddErrorToast($"خطا در پردازش پس‌زمینه: {ex.Message}");
                return;
            }
        }

        var request = new StoreRequest(
            Model.Name,
            Model.Slug,
            logoContent,
            logoExt,
            bgContent,
            bgExt,
            Model.IsActive
        );

        if (Id == null)
        {
            await SendRequestAsync<IStoreService>(
                action: (s, ct) => s.CreateStoreAsync(request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            await SendRequestAsync<IStoreService>(
                action: (s, ct) => s.UpdateStoreAsync(Id.Value, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
    }

    private void Close() => MudDialog.Cancel();
}
