using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings;

public partial class BaseInfo
{
    private SettingsVm _model = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadSettingsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response =>
            {
                if (response is not null)
                {
                    _model = SettingsVm.CreateFromRequest(response);
                }
                else
                {
                    AddErrorToast("فراخوانی تنظیمات با مشکل مواجه شد");
                }
            });
    }

    private async Task OnGallerySettingsSubmitted(EditContext context)
    {
        if (_model.IconFile is not null)
        {
            await using var stream = _model.IconFile.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            _model.IconContent = memoryStream.ToArray();
        }

        await SendRequestAsync<ISettingService>(
            action: (s, ct) => s.UpdateAsync(_model.ToRequest(), ct));

        AddSuccessToast("تنظیمات گالری با موفقیت ذخیره شد");

        await LoadSettingsAsync();
        StateHasChanged();
    }
}