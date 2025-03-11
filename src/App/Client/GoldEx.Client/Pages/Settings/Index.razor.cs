using GoldEx.Client.Pages.Settings.ViewModels;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Settings;

public partial class Index
{
    private SettingsVm _model = new();

    private ISettingsClientService SettingsService => GetRequiredService<ISettingsClientService>();

    protected override async Task OnInitializedAsync()
    {
        await LoadSettingsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var response = await SettingsService.GetAsync(CancellationTokenSource.Token);

            if (response is not null)
            {
                _model = SettingsVm.CreateFromRequest(response);
            }
            else
            {
                AddErrorToast("فراخوانی تنظیمات با مشکل مواجه شد");
            }
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }

    private async Task OnGallerySettingsSubmitted(EditContext context)
    {
        try
        {
            SetBusy();
            CancelToken();

            var request = _model.ToRequest();

            await SettingsService.UpdateAsync(_model.Id, request, CancellationTokenSource.Token);

            AddSuccessToast("تنظیمات با موفقیت ذخیره شد");
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }
}