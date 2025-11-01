namespace GoldEx.Shared.DTOs.Settings.Barcodes;

public sealed record UpdateBarcodePrintSettingsRequest(
    int LabelWidth,
    int LabelHeight,
    int MarginTop,
    int MarginRight,
    int MarginBottom,
    int MarginLeft,
    int PaddingTop,
    int PaddingRight,
    int PaddingBottom,
    int PaddingLeft,
    List<BarcodePositionItemRequest> PositionItems
);