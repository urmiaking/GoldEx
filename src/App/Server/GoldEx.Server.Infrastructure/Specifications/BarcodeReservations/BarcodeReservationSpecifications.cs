using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.BarcodeReservationAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.BarcodeReservations;

/// <summary>
/// جستجوی رزرو بارکد بر اساس مقدار دقیق بارکد (صرف نظر از وضعیت)
/// </summary>
public sealed class BarcodeReservationsByBarcodeSpecification(string barcode) : SpecificationBase<BarcodeReservation>(x => x.Barcode == barcode);

/// <summary>
/// رزروی که هنوز فعال است (Reserved و منقضی نشده) بر اساس بارکد
/// </summary>
public sealed class BarcodeActiveReservationByBarcodeSpecification : SpecificationBase<BarcodeReservation>
{
    public BarcodeActiveReservationByBarcodeSpecification(string barcode)
    {
        AddCriteria(x => x.Barcode == barcode);
        AddCriteria(x => x.Status == BarcodeReservationStatus.Reserved);
        AddCriteria(x => x.ExpiresAt > DateTime.Now);
    }
}

/// <summary>
/// همه رزروهای فعال (Reserved و منقضی‌نشده) برای یک پیشوند خاص
/// </summary>
public sealed class BarcodeActiveReservationsByPrefixSpecification : SpecificationBase<BarcodeReservation>
{
    public BarcodeActiveReservationsByPrefixSpecification(string prefix)
    {
        AddCriteria(x => x.Prefix == prefix);
        AddCriteria(x => x.Status == BarcodeReservationStatus.Reserved);
        AddCriteria(x => x.ExpiresAt > DateTime.Now);

        ApplyOrderByDescending(x => x.Barcode);
    }
}

/// <summary>
/// آخرین رزرو فعال برای پیشوند (برای صدور بارکد بعدی بر اساس رزروهای فعلی)
/// </summary>
public sealed class BarcodeLatestActiveReservationByPrefixSpecification : SpecificationBase<BarcodeReservation>
{
    public BarcodeLatestActiveReservationByPrefixSpecification(string prefix)
    {
        AddCriteria(x => x.Prefix == prefix);
        AddCriteria(x => x.Status == BarcodeReservationStatus.Reserved);
        AddCriteria(x => x.ExpiresAt > DateTime.Now);

        ApplyOrderByDescending(x => x.Barcode);
        ApplyPaging(0, 1);
    }
}

/// <summary>
/// آخرین رزرو فعال یا تعهدشده برای پیشوند
/// - برای جلوگیری از صدور بارکد تکراری در شرایطی که Committed شده ولی هنوز در Products ثبت نشده
/// </summary>
public sealed class BarcodeLatestActiveOrCommittedByPrefixSpecification : SpecificationBase<BarcodeReservation>
{
    public BarcodeLatestActiveOrCommittedByPrefixSpecification(string prefix)
    {
        AddCriteria(x => x.Prefix == prefix);
        AddCriteria(x =>
            (x.Status == BarcodeReservationStatus.Reserved && x.ExpiresAt > DateTime.Now) ||
            x.Status == BarcodeReservationStatus.Committed);

        ApplyOrderByDescending(x => x.Barcode);
        ApplyPaging(0, 1);
    }
}

/// <summary>
/// رزروهایی که منقضی شده‌اند (Reserved و تاریخ انقضا گذشته)
/// برای سرویس پاکسازی/به‌روزرسانی وضعیت
/// </summary>
public sealed class BarcodeExpiredReservationsSpecification : SpecificationBase<BarcodeReservation>
{
    public BarcodeExpiredReservationsSpecification()
    {
        AddCriteria(x => x.Status == BarcodeReservationStatus.Reserved);
        AddCriteria(x => x.ExpiresAt <= DateTime.Now);

        ApplyOrderBy(x => x.ExpiresAt);
    }
}

/// <summary>
/// همه رزروهای مربوط به یک فاکتور خاص (برای Commit/Release گروهی)
/// </summary>
public sealed class ReservationsByInvoiceSpecification : SpecificationBase<BarcodeReservation>
{
    public ReservationsByInvoiceSpecification(InvoiceId invoiceId)
    {
        AddCriteria(x => x.InvoiceId == invoiceId);
    }
}