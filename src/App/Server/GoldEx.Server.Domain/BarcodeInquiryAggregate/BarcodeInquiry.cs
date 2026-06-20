using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.BarcodeInquiryAggregate;

public readonly record struct BarcodeInquiryId(Guid Value);
public class BarcodeInquiry : EntityBase<BarcodeInquiryId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }
    public string Barcode { get; private set; }
    public DateTime InquiryDate { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private BarcodeInquiry() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private BarcodeInquiry(string barcode, StoreId storeId = default) : base(new BarcodeInquiryId(Guid.CreateVersion7()))
    {
        Barcode = barcode;
        InquiryDate = DateTime.Now;
        StoreId = storeId;
    }

    public static BarcodeInquiry Create(string barcode, StoreId storeId = default)
    {
        return new BarcodeInquiry(barcode, storeId);
    }

    public void SetInquiryDate()
    {
        InquiryDate = DateTime.Now;
    }
}