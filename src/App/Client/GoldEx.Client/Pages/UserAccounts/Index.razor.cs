using GoldEx.Client.Pages.UserAccounts.Components.Editors;
using GoldEx.Shared.Routings;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: Icons.Material.Filled.ManageAccounts)
    ];

    private async Task ChangePassword()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.ExtraSmall };

        var dialog = await DialogService.ShowAsync<PasswordEditor>("تغییر رمز عبور", dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("رمز عبور با موفقیت ویرایش شد.");
            await Task.Delay(1500);
            Navigation.NavigateTo(ClientRoutes.UserAccounts.Index, true);
        }
    }
}