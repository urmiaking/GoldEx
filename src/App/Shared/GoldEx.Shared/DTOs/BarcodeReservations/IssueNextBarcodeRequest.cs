using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.BarcodeReservations;

/// <summary>
/// درخواست صدور/رزرو بارکد بعدی
/// </summary>
public sealed record IssueNextBarcodeRequest(
    ProductType ProductType,
    Guid? ProductCategoryId,
    Guid? InvoiceId);