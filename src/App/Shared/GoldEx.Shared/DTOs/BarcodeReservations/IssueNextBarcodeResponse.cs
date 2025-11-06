namespace GoldEx.Shared.DTOs.BarcodeReservations;

/// <summary>
/// نتیجه صدور بارکد بعدی (با رزرو)
/// </summary>
/// <param name="Prefix">پیشوند کامل (نوع + کد دسته)</param>
/// <param name="Barcode">بارکد رزرو شده</param>
/// <param name="ExpiresAt">زمان پایان اعتبار رزرو</param>
public sealed record IssueNextBarcodeResponse(string Prefix, string Barcode, DateTime ExpiresAt);