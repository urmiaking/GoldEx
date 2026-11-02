using MudBlazor;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Components.Services.Abstractions;

public interface IThemeService
{
    bool IsDarkMode { get; }
    string CurrentPalette { get; }
    MudTheme CurrentTheme { get; }
    bool IsChanging { get; }
    event Func<Task>? OnAppearanceChanged;
    void ToggleMode();
    Task ToggleModeAsync(ElementReference source);
    Task SetPaletteAsync(string paletteName, ElementReference source);
    Task LoadSettingsAsync();
    Task SyncBrowserAsync();
}
