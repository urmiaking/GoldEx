using GoldEx.Client.Pages.Products.ViewModels;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.DTOs.Stores;
using GoldEx.Shared.DTOs.Vitrine;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;

namespace GoldEx.Client.Pages.Products.Components;

public partial class VitrineQuickEditDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public ProductVm Model { get; set; } = new();
    [Inject] private HttpClient HttpClient { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private bool _isUploading;
    private bool _isSaving;
    private UserStoreDto? _currentStore;
    private string PublicVitrineUrl { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        var stores = await SendRequestAsync<IStoreService, List<UserStoreDto>>(
            action: (service, token) => service.GetUserStoresAsync(token));

        _currentStore = stores?.FirstOrDefault(s => s.IsCurrent) ?? stores?.FirstOrDefault();
        UpdatePublicVitrineUrl();
    }

    private void UpdatePublicVitrineUrl()
    {
        if (Model == null || string.IsNullOrWhiteSpace(Model.Barcode)) return;
        PublicVitrineUrl = VitrineUrlHelper.BuildProductVitrineUrl(
            _currentStore?.CustomDomain,
            Navigation.BaseUri,
            _currentStore?.Slug ?? "default",
            Model.Barcode);
    }

    private async Task CopyVitrineUrlAsync()
    {
        if (string.IsNullOrWhiteSpace(PublicVitrineUrl)) return;
        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", PublicVitrineUrl);
        AddSuccessToast($"لینک عمومی کالا کپی شد: {PublicVitrineUrl}");
    }

    private void SetMainImage(ProductImageDto target)
    {
        Model.Images = Model.Images.Select(img => img with { IsMain = img.Url == target.Url }).ToList();
    }

    private void RemoveImage(ProductImageDto target)
    {
        Model.Images = Model.Images.Where(img => img.Url != target.Url).ToList();
        if (target.IsMain && Model.Images.Count > 0)
        {
            Model.Images[0] = Model.Images[0] with { IsMain = true };
        }
    }

    private async Task UploadImageAsync(IBrowserFile file)
    {
        if (file == null) return;

        _isUploading = true;
        try
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 20_000_000));
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, "file", file.Name);

            var response = await HttpClient.PostAsync(ApiUrls.Vitrine.UploadProductImage(), content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<UploadImageResponse>();
                if (result != null && !string.IsNullOrEmpty(result.Url))
                {
                    var isFirst = Model.Images.Count == 0;
                    Model.Images.Add(new ProductImageDto(result.Url, IsMain: isFirst, DisplayOrder: Model.Images.Count));
                    AddSuccessToast("تصویر با موفقیت بارگذاری شد.");
                }
            }
            else
            {
                var err = await response.Content.ReadAsStringAsync();
                AddErrorToast(string.IsNullOrEmpty(err) ? "خطا در بارگذاری تصویر." : err);
            }
        }
        catch (Exception ex)
        {
            AddErrorToast($"خطا در آپلود: {ex.Message}");
        }
        finally
        {
            _isUploading = false;
        }
    }

    private async Task SaveAsync()
    {
        if (!Model.Id.HasValue)
        {
            MudDialog.Close(DialogResult.Ok(Model));
            return;
        }

        _isSaving = true;
        try
        {
            var request = new UpdateProductVitrineRequest(
                Model.ShowInVitrine,
                Model.IsFeatured,
                Model.VitrineDescription,
                Model.Images);

            await SendRequestAsync<IVitrineService>(
                action: (service, token) => service.UpdateProductVitrineAsync(Model.Id.Value, request, token),
                afterSend: () =>
                {
                    AddSuccessToast("تنظیمات ویترین با موفقیت ذخیره شد.");
                    MudDialog.Close(DialogResult.Ok(Model));
                    return Task.CompletedTask;
                });
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();

    private record UploadImageResponse(string Url);
}
