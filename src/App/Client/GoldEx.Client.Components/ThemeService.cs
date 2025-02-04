namespace GoldEx.Client.Components;

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

public interface IThemeService
{
    bool IsDarkMode { get; }
    void ToggleMode();
}
