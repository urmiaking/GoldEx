namespace GoldEx.Shared.DTOs.BarcodeReservations;

/// <summary>
/// نتیجه صدور بارکد بعدی (با رزرو)
/// </summary>
/// <param name="Barcode">بارکد رزرو شده</param>
public sealed record IssueNextBarcodeResponse(string Barcode);