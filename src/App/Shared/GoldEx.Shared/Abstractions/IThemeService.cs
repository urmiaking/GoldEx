namespace GoldEx.Shared.Abstractions;

public interface IThemeService
{
    bool IsDarkMode { get; }
    void ToggleMode();
}