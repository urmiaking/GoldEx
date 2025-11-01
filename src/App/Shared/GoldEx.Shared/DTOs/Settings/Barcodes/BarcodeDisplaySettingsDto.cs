namespace GoldEx.Shared.DTOs.Settings.Barcodes;

public sealed record BarcodeDisplaySettingsDto(
    int Width,
    int Height,
    bool DisplayValue,
    int FontSize,
    int Margin
);