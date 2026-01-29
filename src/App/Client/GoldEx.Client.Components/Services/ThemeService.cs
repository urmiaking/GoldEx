using Blazored.LocalStorage;
using GoldEx.Client.Components.Constants;
using GoldEx.Client.Components.Services.Abstractions;
using GoldEx.Client.Components.Themes;
using MudBlazor;

namespace GoldEx.Client.Components.Services;

public class ThemeService(ILocalStorageService localStorage) : IThemeService
{
    private string _currentPalette = "Gold";
    private bool _isDarkMode;

    // Properties  
    public bool IsDarkMode => _isDarkMode;
    public string CurrentPalette => _currentPalette;
    public MudTheme CurrentTheme => ColorPalettes.Palettes[_currentPalette];

    // Events  
    public event EventHandler? OnToggleMode;
    public event EventHandler? OnPaletteChanged;

    // Methods  
    public void ToggleMode()
    {
        _isDarkMode = !_isDarkMode;

        // Only persist if JS/localStorage is available
        if (OperatingSystem.IsBrowser())
        {
            _ = localStorage.SetItemAsStringAsync(LocalStorageKeys.IsDarkMode, _isDarkMode.ToString());
        }

        OnToggleMode?.Invoke(this, EventArgs.Empty);
    }

    public async Task SetPaletteAsync(string paletteName)
    {
        if (ColorPalettes.Palettes.ContainsKey(paletteName))
        {
            _currentPalette = paletteName;

            if (OperatingSystem.IsBrowser())
            {
                await localStorage.SetItemAsStringAsync(LocalStorageKeys.SelectedPalette, paletteName);
            }

            OnPaletteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public async Task LoadSettingsAsync()
    {
        if (!OperatingSystem.IsBrowser())
        {
            // Running during prerender/server: just stick to defaults
            _currentPalette = "Gold";
            _isDarkMode = false;
            return;
        }

        var savedPalette = await localStorage.GetItemAsStringAsync(LocalStorageKeys.SelectedPalette);
        if (!string.IsNullOrEmpty(savedPalette) && ColorPalettes.Palettes.ContainsKey(savedPalette))
        {
            _currentPalette = savedPalette;
        }

        var savedDarkMode = await localStorage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode);
        if (!string.IsNullOrEmpty(savedDarkMode) && bool.TryParse(savedDarkMode, out var isDarkMode))
        {
            _isDarkMode = isDarkMode;
        }
    }
}
