using GoldEx.Client.Components.Extensions;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class Profile
{
    private bool _isProfileOpen;
    private string? _currentUrl;
    private Color _color = Color.Dark;
    private string? _username;
    private string _status = "در حال بررسی...";

    private IHealthService HealthService => GetRequiredService<IHealthService>();

    private async Task CheckServerHealthAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var response = await HealthService.GetAsync(CancellationTokenSource.Token);

            _color = response.Status == "Healthy" ? Color.Success : Color.Warning;
            _status = response.Status == "Healthy" ? "آنلاین" : "اختلال در سرویس ها";
        }
        catch
        {
            _color = Color.Error;
            _status = "آفلاین";
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

    private void NavigateToAccountManagement()
    {
        Navigation.NavigateTo(ClientRoutes.Accounts.Manage.Index, forceLoad: true);
    }

    protected override void OnInitialized()
    {
        _currentUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
        Navigation.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _currentUrl = Navigation.ToBaseRelativePath(e.Location);
        StateHasChanged();
    }

    public override void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
        base.Dispose();
    }
}