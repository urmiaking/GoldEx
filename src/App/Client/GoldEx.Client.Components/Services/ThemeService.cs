using Blazored.LocalStorage;
using GoldEx.Client.Components.Constants;
using GoldEx.Client.Components.Services.Abstractions;
using GoldEx.Client.Components.Themes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace GoldEx.Client.Components.Services;

public sealed class ThemeService(ILocalStorageService localStorage, IJSRuntime js) : IThemeService, IAsyncDisposable
{
    private IJSObjectReference? _module;
    private DotNetObjectReference<ThemeService>? _reference;

    public bool IsDarkMode { get; private set; }
    public string CurrentPalette { get; private set; } = "Gold";
    public MudTheme CurrentTheme => ColorPalettes.Palettes[CurrentPalette];
    public bool IsChanging { get; private set; }
    public event Func<Task>? OnAppearanceChanged;

    // Compatibility for the standalone calculator, which does not host the animated app shell.
    public void ToggleMode() => _ = ToggleLegacyModeAsync();

    private async Task ToggleLegacyModeAsync()
    {
        await ApplyAppearanceAsync(!IsDarkMode, CurrentPalette);
        try { await SyncBrowserAsync(); }
        catch (JSException) { /* The selected MudBlazor mode has already been applied. */ }
    }

    public Task ToggleModeAsync(ElementReference source) => ChangeAsync(!IsDarkMode, CurrentPalette, source);

    public Task SetPaletteAsync(string paletteName, ElementReference source) =>
        ColorPalettes.Palettes.ContainsKey(paletteName)
            ? ChangeAsync(IsDarkMode, paletteName, source)
            : Task.CompletedTask;

    private async Task ChangeAsync(bool dark, string palette, ElementReference source)
    {
        if (IsChanging || (dark == IsDarkMode && palette == CurrentPalette)) return;
        IsChanging = true;
        try
        {
            await NotifyAsync();
            _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./_content/GoldEx.Client.Components/js/appearance.js");
            _reference ??= DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("change", _reference, dark, palette, source);
        }
        catch (JSException)
        {
            // Snapshot failures must not prevent an appearance change.
            await ApplyAppearanceAsync(dark, palette);
        }
        finally
        {
            IsChanging = false;
            await NotifyAsync();
        }
    }

    [JSInvokable]
    public async Task ApplyAppearanceAsync(bool dark, string palette)
    {
        if (!ColorPalettes.Palettes.ContainsKey(palette)) return;
        IsDarkMode = dark;
        CurrentPalette = palette;
        // Await the layout render so the snapshot includes the new MudThemeProvider stylesheet.
        await NotifyAsync();
        try
        {
            await localStorage.SetItemAsStringAsync(LocalStorageKeys.IsDarkMode, dark.ToString());
            await localStorage.SetItemAsStringAsync(LocalStorageKeys.SelectedPalette, palette);
        }
        catch (JSException) { /* Appearance still works when browser storage is blocked. */ }
    }

    private async Task NotifyAsync()
    {
        if (OnAppearanceChanged is null) return;
        foreach (Func<Task> handler in OnAppearanceChanged.GetInvocationList()) await handler();
    }

    public async Task LoadSettingsAsync()
    {
        if (!OperatingSystem.IsBrowser()) return;
        try
        {
            var palette = await localStorage.GetItemAsStringAsync(LocalStorageKeys.SelectedPalette);
            if (palette is not null && ColorPalettes.Palettes.ContainsKey(palette)) CurrentPalette = palette;
            var mode = await localStorage.GetItemAsStringAsync(LocalStorageKeys.IsDarkMode);
            if (bool.TryParse(mode, out var dark)) IsDarkMode = dark;
        }
        catch (JSException) { /* Use gold/light defaults if browser storage is unavailable. */ }
    }

    public async Task SyncBrowserAsync()
    {
        _module ??= await js.InvokeAsync<IJSObjectReference>("import", "./_content/GoldEx.Client.Components/js/appearance.js");
        await _module.InvokeVoidAsync("sync", IsDarkMode, CurrentPalette);
    }

    public async ValueTask DisposeAsync()
    {
        _reference?.Dispose();
        try { if (_module is not null) await _module.DisposeAsync(); }
        catch (JSDisconnectedException) { }
    }
}
