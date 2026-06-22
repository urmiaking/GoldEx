using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.Settings.Barcodes;

public sealed record GetBarcodePositionItemResponse(
    Guid Id,
    BarcodePosition Position,
    BarcodePrintableItem ItemType,
    int Order,
    bool IsVisible,
    double FontSize,
    double ItemSpacing,
    BarcodeDisplaySettingsDto? BarcodeSettings
);