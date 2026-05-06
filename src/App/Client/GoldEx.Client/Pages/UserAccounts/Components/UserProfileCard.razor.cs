using GoldEx.Client.Pages.UserAccounts.Components.Editors;
using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts.Components;

public partial class UserProfileCard
{
    [PersistentState]
    public GetUserAccountResponse? UserInfo { get; set; }

    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.ExtraSmall };

    protected override async Task OnInitializedAsync()
    {
        if (UserInfo is null) 
            await LoadUserInfoAsync();

        await base.OnInitializedAsync();
    }

    private async Task LoadUserInfoAsync()
    {
        UserInfo = await SendRequestAsync<IUserAccountService, GetUserAccountResponse>(
            action: (s, ct) => s.GetCurrentUserInfoAsync(ct),
            createScope: true);
    }

    private async Task ChangeFullName()
    {
        if (UserInfo is null)
            return;

        var parameters = new DialogParameters<FullNameEditor>
        {
            { x => x.FullName, UserInfo.FullName }
        };

        var dialog = await DialogService.ShowAsync<FullNameEditor>("ویرایش مشخصات فردی", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("اطلاعات فردی با موفقیت ویرایش شد.");
            await Task.Delay(1500);
            Navigation.NavigateTo(ClientRoutes.UserAccounts.Index, true);
        }
    }

    private async Task ChangePhoneNumber()
    {
        if (UserInfo is null)
            return;

        var parameters = new DialogParameters<PhoneNumberEditor>
        {
            { x => x.PhoneNumber, UserInfo.PhoneNumber }
        };

        var dialog = await DialogService.ShowAsync<PhoneNumberEditor>("ویرایش شماره تلفن", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("شماره تلفن با موفقیت ویرایش شد.");
            await Task.Delay(1500);
            Navigation.NavigateTo(ClientRoutes.UserAccounts.Index, true);
        }
    }

    private async Task ChangeEmail()
    {
        if (UserInfo is null)
            return;

        var parameters = new DialogParameters<EmailEditor>
        {
            { x => x.Email, UserInfo.Email }
        };

        var dialog = await DialogService.ShowAsync<EmailEditor>("ویرایش ایمیل", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("آدرس ایمیل با موفقیت ویرایش شد.");
            await LoadUserInfoAsync();
        }
    }

    private async Task ChangePassword()
    {
        var dialog = await DialogService.ShowAsync<PasswordEditor>("تغییر رمز عبور", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("رمز عبور با موفقیت ویرایش شد.");
            await Task.Delay(1500);
            Navigation.NavigateTo(ClientRoutes.UserAccounts.Index, true);
        }
    }
}