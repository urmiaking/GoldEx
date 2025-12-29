using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings;

public partial class BaseInfo
{
    private SettingsVm _model = new();
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "settings-video";
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadSettingsAsync();
        await base.OnInitializedAsync();
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
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
            await using var stream = _model.IconFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024); // 5 MB
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