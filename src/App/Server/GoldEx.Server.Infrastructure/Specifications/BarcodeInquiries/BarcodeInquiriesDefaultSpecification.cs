using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.BarcodeInquiryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.BarcodeInquiries;

public class BarcodeInquiriesDefaultSpecification : SpecificationBase<BarcodeInquiry>
{
    public BarcodeInquiriesDefaultSpecification(string? barcode)
    {
        if (!string.IsNullOrWhiteSpace(barcode))
        {
            AddCriteria(x => x.Barcode.StartsWith(barcode));
        }

        ApplyOrderByDescending(x => x.InquiryDate);

        ApplyPaging(0, 5);
    }
}