using GoldEx.Shared.DTOs.UserAccounts;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.UserAccounts;

public partial class ExternalLogins
{
    [SupplyParameterFromQuery(Name = "result")] public string? LinkResult { get; set; }

    private readonly List<BreadcrumbItem> _breadcrumbs =
    [
        new("صفحه اصلی", href: ClientRoutes.Home.Index, icon: Icons.Material.Filled.Home),
        new("مدیریت حساب", href: ClientRoutes.UserAccounts.Index, icon: Icons.Material.Filled.Person),
        new("احراز هویت خارجی", href: ClientRoutes.UserAccounts.ExternalLogins, icon: Icons.Material.Filled.AddLink)
    ];

    private GetExternalProviderResponse? _externalLogins;
    private bool _buttonDisabled;

    protected override async Task OnInitializedAsync()
    {
        await LoadExternalProvidersAsync();
        await base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        CheckLinkResult();
        base.OnAfterRender(firstRender);
    }

    private void CheckLinkResult()
    {
        if (string.IsNullOrEmpty(LinkResult)) 
            return;

        var success = Enum.TryParse<ExternalLoginResult>(LinkResult, true, out var result);

        if (!success)
        {
            AddErrorToast("Conversion failed");
            return;
        }

        switch (result)
        {
            case ExternalLoginResult.NoInfo:
                AddErrorToast("خطا در ثبت سرویس");
                break;
            case ExternalLoginResult.Success:
                AddSuccessToast("حساب شما با موفقیت به سرویس احراز هویت متصل شد");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task LoadExternalProvidersAsync()
    {
        _externalLogins = await SendRequestAsync<IUserAccountService, GetExternalProviderResponse>(
            action: (s, ct) => s.GetExternalProvidersAsync(ct));
    }

    private static string GetProviderIcon(string provider)
    {
        return provider.ToLower() switch
        {
            "google" => Icons.Custom.Brands.Google,
            "microsoft" => Icons.Custom.Brands.Microsoft,
            "github" => Icons.Custom.Brands.GitHub,
            _ => Icons.Material.Filled.AccountCircle
        };
    }

    private Task StartExternalLogin(string provider)
    {
        Navigation.NavigateTo(ApiUrls.UserAccounts.ExternalLogin(provider), forceLoad: true);
        _buttonDisabled = true;
        return Task.CompletedTask;
    }
}