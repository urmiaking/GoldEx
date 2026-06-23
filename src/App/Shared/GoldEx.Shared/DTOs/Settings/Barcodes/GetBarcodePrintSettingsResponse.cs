namespace GoldEx.Shared.DTOs.Settings.Barcodes;

public sealed record GetBarcodePrintSettingsResponse(
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
    List<GetBarcodePositionItemResponse> PositionItems
);