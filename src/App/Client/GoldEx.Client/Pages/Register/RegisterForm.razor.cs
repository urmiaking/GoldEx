using GoldEx.Client.Pages.Register.ViewModels;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components.Forms;

namespace GoldEx.Client.Pages.Register;

public partial class RegisterForm
{
    private bool _tokenSent;
    private bool _sendTokenDisabled;
    private int _otpRemaining;
    private CancellationTokenSource? _otpCts;
    private RegisterFormVm _model = new ();

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
                    _model = RegisterFormVm.CreateFrom(response);
                }
                else
                {
                    AddErrorToast("فراخوانی تنظیمات با مشکل مواجه شد");
                }
            });
    }


    private async Task OnSubmit(EditContext context)
    {
        if (_model.IconFile is not null)
        {
            await using var stream = _model.IconFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024); // 5 MB
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            _model.IconContent = memoryStream.ToArray();
        }

        var request = _model.ToRequest();

        await SendRequestAsync<ILicenseService>(
            action: (s, ct) => s.RegisterProductAsync(request, ct),
            afterSend: () =>
            {
                AddSuccessToast("نرم افزار با موفقیت فعال شد");
                Navigation.NavigateTo(ClientRoutes.Home.Index, forceLoad: true);
                return Task.CompletedTask;
            });
    }

    private async Task OnSendToken()
    {
        _tokenSent = true;
        _sendTokenDisabled = true;
        StartOtpTimer();

        await SendRequestAsync<ILicenseService>(
            action: (s, ct) => s.SendTokenAsync(_model.PhoneNumber, ct),
            afterSend: () =>
            {
                AddSuccessToast("کد یک‌ بار مصرف ارسال شد");
                return Task.CompletedTask;
            });
    }

    private async void StartOtpTimer()
    {
        _otpRemaining = 60;

        _otpCts?.Cancel();
        _otpCts = new CancellationTokenSource();

        try
        {
            while (_otpRemaining > 0)
            {
                await Task.Delay(1000, _otpCts.Token);
                _otpRemaining--;
                await InvokeAsync(StateHasChanged);
            }
        }
        catch
        {
            // ignored
        }

        _sendTokenDisabled = false;
        await InvokeAsync(StateHasChanged);
    }
}