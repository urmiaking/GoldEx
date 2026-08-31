namespace GoldEx.Shared.DTOs.Vitrine;

public record VitrineThemeDto(
    string Preset = "royal-emerald",
    string? PrimaryColor = null,
    string? AccentColor = null,
    string? BackgroundColor = null,
    string? SurfaceColor = null,
    string CardStyle = "minimal",
    string RadiusStyle = "rounded",
    string FontStyle = "iransans",
    string HeaderStyle = "glass-sticky");
