namespace GoldEx.Shared.Services.Abstractions;

public interface IThemeService
{
    bool IsDarkMode { get; }
    void ToggleMode();
}