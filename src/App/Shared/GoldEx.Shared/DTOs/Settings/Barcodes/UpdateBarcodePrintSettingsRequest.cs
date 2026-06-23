namespace GoldEx.Shared.DTOs.Settings.Barcodes;

public sealed record UpdateBarcodePrintSettingsRequest(
    double LabelWidth,
    double LabelHeight,
    double TailWidth,
    double MarginTop,
    double MarginRight,
    double MarginBottom,
    double MarginLeft,
    double PaddingTop,
    double PaddingRight,
    double PaddingBottom,
    double PaddingLeft,
    List<BarcodePositionItemRequest> PositionItems
);