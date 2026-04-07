using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts;

public partial class TwoFactorAuthentication
{
    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: Icons.Material.Filled.Person),
        new("رمز دو مرحله ای", href: ClientRoutes.UserAccounts.TwoFactorAuthentication, icon: Icons.Material.Filled.Password)
    ];

    [PersistentState]
    public GetTwoFactorAuthStatusResponse? TwoFactorAuthStatus { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (TwoFactorAuthStatus is null) 
            await LoadAuthStatusAsync();

        await base.OnInitializedAsync();
    }

    private async Task LoadAuthStatusAsync()
    {
        TwoFactorAuthStatus = await SendRequestAsync<IUserAccountService, GetTwoFactorAuthStatusResponse>(
            action: (s, ct) => s.GetUser2FaStatusAsync(ct));
    }

    private async Task OnForgetBrowser()
    {
        var result = await DialogService.ShowMessageBoxAsync("فراموشی این دستگاه",
            "از حذف این دستگاه از لیست دستگاه های مجاز برای دور زدن رمز دو مرحله ای اطمینان دارید؟", "بله", "انصراف");

        if (result == true) 
            await SendRequestAsync<IUserAccountService>(action: (s, ct) => s.ForgetDeviceAsync(ct));
    }

    private async Task OnDisable2Fa()
    {
        var result = await DialogService.ShowMessageBoxAsync("غیرفعالسازی رمز دو مرحله ای",
            "با غیرفعالسازی رمز دو مرحله ای امنیت حساب شما کاهش می یابد. آیا اطمینان دارید؟", "بله", "انصراف");

        if (result == true) 
            await SendRequestAsync<IUserAccountService>(action: (s, ct) => s.Disable2FaAsync(ct));
    }
}