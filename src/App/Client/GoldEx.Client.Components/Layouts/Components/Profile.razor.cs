using GoldEx.Client.Components.Extensions;
using GoldEx.Client.Components.Services.Abstractions;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MudBlazor;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class Profile
{
    private bool _isProfileOpen;
    private bool _isDarkMode;
    private string? _currentUrl;
    private Color _color = Color.Dark;
    private string? _username;
    private string _status = "در حال بررسی...";
    private bool _healthMonitorOpen;
    private GetLicenseResponse? _licenseInfo;

    [Inject] private IThemeService? ThemeService { get; set; }

    private IHealthService HealthService => GetRequiredService<IHealthService>();

    private async Task CheckServerHealthAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var response = await HealthService.GetAsync(CancellationTokenSource.Token);

            _color = response.Status is HealthStatus.Healthy ? Color.Success : Color.Warning;
            _status = response.Status == HealthStatus.Healthy ? "آنلاین" : "اختلال در سرویس ها";
        }
        catch
        {
            _color = Color.Success;
            _status = "آنلاین";
        }
        finally
        {
            SetIdeal();
        }
    }

    private async Task ToggleProfile()
    {
        _isProfileOpen = !_isProfileOpen;

        if (_isProfileOpen)
        {
            _username = User?.GetDisplayName();
            await CheckServerHealthAsync();
            await LoadLicenseAsync();
        }
        else
        {
            _color = Color.Dark;
        }
    }

    private void NavigateToAccountManagement()
    {
        Navigation.NavigateTo(ClientRoutes.Accounts.Manage.Index, forceLoad: true);
    }

    protected override void OnInitialized()
    {
        _isDarkMode = ThemeService?.IsDarkMode ?? false;

        _currentUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
        Navigation.LocationChanged += OnLocationChanged;
    }

    private async Task LoadLicenseAsync()
    {
        await SendRequestAsync<ILicenseService, GetLicenseResponse>(
            action: (s, ct) => s.GetLicenseAsync(ct),
            afterSend: response => _licenseInfo = response,
            createScope: true);
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _currentUrl = Navigation.ToBaseRelativePath(e.Location);
        StateHasChanged();
    }

    private async Task OnPaletteSelected(string paletteName)
    {
        if (ThemeService is not null)
        {
            await ThemeService.SetPaletteAsync(paletteName);
        }
    }

    public override ValueTask DisposeAsync()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        return base.DisposeAsync();
    }

    private void OnDarkModeToggleChanged(bool value)
    {
        if (ThemeService is not null)
        {
            _isDarkMode = value;
            ThemeService.ToggleMode();

            StateHasChanged();
        }
    }

    private Color GetLicenseColor(GetLicenseResponse licenseInfo)
    {
        return licenseInfo.Plan switch {
            LicensePlan.Unregistered => Color.Error,
            LicensePlan.Trial => Color.Info,
            LicensePlan.Regular => Color.Success,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private double GetLicenseProgress(GetLicenseResponse license)
    {
        if (license.Plan == LicensePlan.Unregistered)
            return 0;

        if (license.ExpireDate == DateTime.MinValue)
            return 0;

        var now = DateTime.UtcNow.Date;
        var start = license.RegisteredAt.Date;
        var end = license.ExpireDate.Date;

        if (now >= end)
            return 0;

        var totalDays = (end - start).TotalDays;
        if (totalDays <= 0)
            return 0;

        var remainingDays = (end - now).TotalDays;

        return Math.Clamp((remainingDays / totalDays) * 100, 0, 100);
    }

}