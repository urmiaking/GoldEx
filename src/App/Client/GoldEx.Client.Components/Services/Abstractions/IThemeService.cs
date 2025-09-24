using MudBlazor;

namespace GoldEx.Client.Components.Services.Abstractions;

public interface IThemeService
{
    bool IsDarkMode { get; }
    string CurrentPalette { get; }
    MudTheme CurrentTheme { get; }
    event EventHandler OnToggleMode;
    event EventHandler OnPaletteChanged;

    void ToggleMode();
    Task SetPaletteAsync(string paletteName);
    Task LoadSettingsAsync();
}