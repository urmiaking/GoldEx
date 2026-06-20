using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.BarcodeReservationAggregate;

public readonly record struct BarcodeReservationId(Guid Value);

public sealed class BarcodeReservation : EntityBase<BarcodeReservationId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }
    public BarcodeType BarcodeType { get; private set; }
    public string? Prefix { get; private set; }
    public string Barcode { get; private set; }
    public BarcodeReservationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public InvoiceId? InvoiceId { get; private set; }

#pragma warning disable CS8618
    private BarcodeReservation() { }
#pragma warning restore CS8618

    private BarcodeReservation(
        BarcodeType barcodeType,
        string? prefix,
        string barcode,
        TimeSpan ttl,
        InvoiceId? invoiceId = null,
        StoreId storeId = default)
    {
        Id = new BarcodeReservationId(Guid.CreateVersion7());
        BarcodeType = barcodeType;
        Prefix = string.IsNullOrEmpty(prefix) ? null : Normalize(prefix);
        Barcode = Normalize(barcode);
        Status = BarcodeReservationStatus.Reserved;
        ExpiresAt = DateTime.Now.Add(ttl);
        InvoiceId = invoiceId;
        StoreId = storeId;
    }

    public static BarcodeReservation Create(BarcodeType barcodeType, string? prefix, string barcode, TimeSpan ttl, InvoiceId? invoiceId = null, StoreId storeId = default)
    {
        ValidateBarcode(barcode);

        return new BarcodeReservation(barcodeType, prefix, barcode, ttl, invoiceId, storeId);
    }

    public void Commit(InvoiceId? invoiceId)
    {
        EnsureStatus(BarcodeReservationStatus.Reserved);

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

    private static void ValidateBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            throw new InvalidOperationException("بارکد نامعتبر است");

        var b = Normalize(barcode);
        if (b.Length is < 4 or > 16)
            throw new InvalidOperationException("طول بارکد نامعتبر است");
    }

    private void EnsureStatus(BarcodeReservationStatus expected)
    {
        if (Status != expected)
            throw new InvalidOperationException("تغییر وضعیت رزرو مجاز نیست");
    }
}