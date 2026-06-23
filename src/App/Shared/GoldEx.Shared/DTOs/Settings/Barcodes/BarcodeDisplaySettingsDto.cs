namespace GoldEx.Shared.DTOs.Settings.Barcodes;

public sealed record BarcodeDisplaySettingsDto(
    double Width,
    double Height,
    bool DisplayValue,
    double FontSize,
    double Margin,
    int BarWidthMultiplier
);