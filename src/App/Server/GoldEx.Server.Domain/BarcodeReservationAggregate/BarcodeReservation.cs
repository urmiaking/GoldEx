using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.BarcodeReservationAggregate;

public readonly record struct BarcodeReservationId(Guid Value);

public sealed class BarcodeReservation : EntityBase<BarcodeReservationId>
{
    public string Prefix { get; private set; }
    public string Barcode { get; private set; }
    public BarcodeReservationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public InvoiceId? InvoiceId { get; private set; }

#pragma warning disable CS8618
    private BarcodeReservation() { }
#pragma warning restore CS8618

    private BarcodeReservation(
        string prefix,
        string barcode,
        TimeSpan ttl,
        InvoiceId? invoiceId = null)
    {
        Id = new BarcodeReservationId(Guid.NewGuid());
        Prefix = Normalize(prefix);
        Barcode = Normalize(barcode);
        Status = BarcodeReservationStatus.Reserved;
        ExpiresAt = DateTime.Now.Add(ttl);
        InvoiceId = invoiceId;
    }

    public static BarcodeReservation Create(string prefix, string barcode, TimeSpan ttl, InvoiceId? invoiceId = null)
    {
        ValidatePrefix(prefix);
        ValidateBarcode(barcode);

        return new BarcodeReservation(prefix, barcode, ttl, invoiceId);
    }

    public bool IsActive() => Status == BarcodeReservationStatus.Reserved && DateTime.Now < ExpiresAt;

    public void Renew(TimeSpan ttl)
    {
        var now = DateTime.Now;
        EnsureStatus(BarcodeReservationStatus.Reserved);
        if (now >= ExpiresAt) 
            throw new InvalidOperationException("رزرو منقضی شده است و امکان تمدید وجود ندارد");
        ExpiresAt = now.Add(ttl);
    }

    public void Commit(InvoiceId? invoiceId)
    {
        EnsureStatus(BarcodeReservationStatus.Reserved);
        if (DateTime.Now >= ExpiresAt)
            throw new InvalidOperationException("زمان رزرو بارکد به پایان رسیده است");
        InvoiceId = invoiceId;
        Status = BarcodeReservationStatus.Committed;
    }

    public void Release()
    {
        EnsureStatus(BarcodeReservationStatus.Reserved);
        Status = BarcodeReservationStatus.Released;
    }

    public void Expire()
    {
        if (Status == BarcodeReservationStatus.Reserved && DateTime.Now >= ExpiresAt)
        {
            Status = BarcodeReservationStatus.Expired;
            return;
        }

        // idempotent: allow calling Expire on already expired/released/committed without throwing
        if (Status is BarcodeReservationStatus.Expired or BarcodeReservationStatus.Released or BarcodeReservationStatus.Committed)
            return;

        throw new InvalidOperationException("رزرو در وضعیت نامعتبر برای انقضا است");
    }

    private static string Normalize(string input) => input.Trim().ToUpperInvariant();

    private static void ValidatePrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new InvalidOperationException("پیشوند بارکد نامعتبر است");

        var p = Normalize(prefix);
        if (p.Length != 3)
            throw new InvalidOperationException("طول پیشوند بارکد نامعتبر است");
    }

    private static void ValidateBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new InvalidOperationException("بارکد نامعتبر است");

        var b = Normalize(barcode);
        if (b.Length is < 6 or > 16)
            throw new InvalidOperationException("طول بارکد نامعتبر است");
    }

    private void EnsureStatus(BarcodeReservationStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException("تغییر وضعیت رزرو مجاز نیست");
    }
}