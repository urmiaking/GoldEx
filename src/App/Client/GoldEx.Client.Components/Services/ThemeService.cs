using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Components.Services;

public class ThemeService : IThemeService
{
    public event EventHandler? OnToggleMode;
    public bool IsDarkMode { get; private set; }

    public void ToggleMode()
    {
        IsDarkMode = !IsDarkMode;
        OnToggleMode?.Invoke(this, EventArgs.Empty);
    }
}
