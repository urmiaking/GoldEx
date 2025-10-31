using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.BarcodeInquiryAggregate;

public readonly record struct BarcodeInquiryId(Guid Value);
public class BarcodeInquiry : EntityBase<BarcodeInquiryId>
{
    public string Barcode { get; private set; }
    public DateTime InquiryDate { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private BarcodeInquiry() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    private BarcodeInquiry(string barcode) : base(new BarcodeInquiryId(Guid.NewGuid()))
    {
        Barcode = barcode;
        InquiryDate = DateTime.Now;
    }

    public static BarcodeInquiry Create(string barcode)
    {
        return new BarcodeInquiry(barcode);
    }

    public void SetInquiryDate()
    {
        InquiryDate = DateTime.Now;
    }
}