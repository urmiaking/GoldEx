using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.Enums;

namespace GoldEx.Shared.Services.Abstractions;

/// <summary>
/// سرویس رزرو بارکد برای صدور آنی بارکد در کلاینت با حفظ ترتیب و یکتا بودن
/// </summary>
public interface IBarcodeReservationService
{
    /// <summary>
    /// صدور بارکد بعدی بر اساس نوع محصول و دسته‌بندی (با رزرو کوتاه‌مدت)
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Prefix, Barcode, ExpiresAt</returns>
    Task<IssueNextBarcodeResponse> IssueNextAsync(
        IssueNextBarcodeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// آزاد کردن رزرو بارکد (در صورت حذف آیتم از فرم قبل از ثبت)
    /// </summary>
    Task ReleaseAsync(BarcodeType barcodeType, string barcode, CancellationToken cancellationToken = default);
}