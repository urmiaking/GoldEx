using GoldEx.Client.Pages.UserAccounts.Components.Editors;
using GoldEx.Client.Pages.UserAccounts.ViewModels;
using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts;

public partial class AccountsList
{
    [PersistentState]
    public List<GetUserAccountResponse>? UserAccounts { get; set; }

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: Icons.Material.Filled.ManageAccounts),
        new("مدیریت کاربران", href: ClientRoutes.UserAccounts.AccountsList, icon: Icons.Material.Filled.People)
    ];

    private readonly DialogOptions _dialogOptions = new ()
    {
        CloseButton = true,
        CloseOnEscapeKey = true,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true
    };

    protected override async Task OnInitializedAsync()
    {
        if (UserAccounts is null)
            await LoadAccountsAsync();

        await base.OnInitializedAsync();
    }

    private async Task LoadAccountsAsync()
    {
        UserAccounts = await SendRequestAsync<IUserAccountService, List<GetUserAccountResponse>>(
            action: (s, ct) => s.GetAccountsListAsync(ct));
    }

    private async Task LockUser(GetUserAccountResponse context)
    {
        var result = await DialogService.ShowMessageBoxAsync("قفل حساب کاربری",
            $"آیا از قفل حساب کاربری {context.FullName} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IUserAccountService>(
                action: (s, ct) => s.LockUserAsync(context.Id, ct),
                afterSend: async () =>
                {
                    await LoadAccountsAsync();
                    AddSuccessToast($"حساب کاربری {context.FullName} با موفقیت قفل شد");
                });
        }
    }

    private async Task UnlockUser(GetUserAccountResponse context)
    {
        var result = await DialogService.ShowMessageBoxAsync("فعالسازی حساب کاربری",
            $"آیا از فعالسازی حساب کاربری {context.FullName} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IUserAccountService>(
                action: (s, ct) => s.UnlockUserAsync(context.Id, ct),
                afterSend: async () =>
                {
                    await LoadAccountsAsync();
                    AddSuccessToast($"حساب کاربری {context.FullName} با موفقیت فعال شد");
                });
        }
    }

    private async Task OnAddAccount()
    {
        var dialog = await DialogService.ShowAsync<UserEditor>("حساب کاربری جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: true }) 
            await LoadAccountsAsync();
    }

    private async Task OnEditAccount(GetUserAccountResponse context)
    {
        var parameters = new DialogParameters<UserEditor>
        {
            { x => x.Model, UserEditorVm.CreateFrom(context) }
        };

        var dialog = await DialogService.ShowAsync<UserEditor>("ویرایش حساب کاربری", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: true })
            await LoadAccountsAsync();
    }

    private async Task OnDeleteAccount(GetUserAccountResponse context)
    {
        var result = await DialogService.ShowMessageBoxAsync("حذف حساب کاربری",
            $"آیا از حذف حساب کاربری {context.FullName} اطمینان دارید؟", "بله", "انصراف");

        if (result == true)
        {
            await SendRequestAsync<IUserAccountService>(
                action: (s, ct) => s.DeleteAccountAsync(context.Id, ct),
                afterSend: async () =>
                {
                    await LoadAccountsAsync();
                    AddSuccessToast($"حساب کاربری {context.FullName} با موفقیت حذف شد");
                });
        }
    }
}