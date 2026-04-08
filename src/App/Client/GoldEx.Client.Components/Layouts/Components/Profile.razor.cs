using GoldEx.Client.Components.Extensions;
using GoldEx.Client.Components.Services.Abstractions;
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
        }
        else
        {
            _color = Color.Dark;
        }
    }

    protected override void OnInitialized()
    {
        _isDarkMode = ThemeService?.IsDarkMode ?? false;

        _currentUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
        Navigation.LocationChanged += OnLocationChanged;
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
}