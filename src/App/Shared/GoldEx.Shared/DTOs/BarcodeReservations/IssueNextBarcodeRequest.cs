using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.BarcodeReservations;

/// <summary>
/// درخواست صدور/رزرو بارکد بعدی
/// </summary>
public sealed record IssueNextBarcodeRequest(
    BarcodeType BarcodeType,
    ProductType? ProductType,
    Guid? ProductCategoryId,
    Guid? InvoiceId);