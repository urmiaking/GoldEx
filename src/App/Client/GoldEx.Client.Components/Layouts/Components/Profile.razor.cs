using GoldEx.Shared.Routings;
using GoldEx.Shared.Services;
using MudBlazor;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class Profile
{
    private bool _isProfileOpen;
    private Color _color = Color.Warning;
    private string _username = "User Name";

    private IHealthClientService HealthService => GetRequiredService<IHealthClientService>();

    private async Task CheckServerHealthAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var response = await HealthService.GetAsync(CancellationTokenSource.Token);

            _color = response.Status == "Healthy" ? Color.Success : Color.Warning;
        }
        catch
        {
            _color = Color.Error;
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
            await CheckServerHealthAsync();
        }
        else
        {
            _color = Color.Warning;
        }
    }

    private void NavigateToProfile()
    {
        Navigation.NavigateTo(ClientRoutes.Accounts.Manage.Index, forceLoad: true);
    }
}