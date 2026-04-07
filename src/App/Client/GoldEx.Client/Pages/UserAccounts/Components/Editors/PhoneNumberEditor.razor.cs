using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts.Components.Editors;

public partial class PhoneNumberEditor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter, EditorRequired] public string PhoneNumber { get; set; }

    private string? _oldPhoneNumber;
    private MudForm _form = default!;
    private string? _token;
    private bool _tokenSent;
    private bool _sendTokenDisabled;
    private int _otpRemaining;
    private CancellationTokenSource? _otpCts;

    private bool PhoneNumberChanged => _oldPhoneNumber != PhoneNumber;

    protected override void OnInitialized()
    {
        _oldPhoneNumber = PhoneNumber;
        base.OnInitialized();
    }

    private void Close() => MudDialog.Cancel();

    private async Task Submit()
    {
        if (string.IsNullOrEmpty(_token))
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        var request = new UpdateUserPhoneNumberRequest(PhoneNumber, _token);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.UpdateUserPhoneNumberAsync(request, ct),
            afterSend: () =>
            {
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private async Task SendToken()
    {
        if(string.IsNullOrEmpty(_oldPhoneNumber))
            return;

        _tokenSent = true;
        _sendTokenDisabled = true;
        StartOtpTimer();

        var request = new SendVerificationCodeRequest(_oldPhoneNumber, PhoneNumber);

        await SendRequestAsync<IUserAccountService>(
            action: (s, ct) => s.SendVerificationTokenAsync(request, ct),
            afterSend: () =>
            {
                AddInfoToast($"کد فعالسازی به شماره {_oldPhoneNumber} ارسال شد");
                return Task.CompletedTask;
            });
    }

    private async void StartOtpTimer()
    {
        _otpRemaining = 120;

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