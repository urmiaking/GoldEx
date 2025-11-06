namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerBarcodeReservationService
{
    /// <summary>
    /// به‌روزرسانی رزروهای منقضی‌شده (تبدیل وضعیت به Expired)
    /// </summary>
    /// <returns>تعداد رزروهای به‌روزرسانی‌شده</returns>
    Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// نهایی‌سازی رزرو بارکد هنگام ثبت فاکتور
    /// </summary>
    Task CommitAsync(string barcode, Guid? invoiceId, CancellationToken cancellationToken = default);
}