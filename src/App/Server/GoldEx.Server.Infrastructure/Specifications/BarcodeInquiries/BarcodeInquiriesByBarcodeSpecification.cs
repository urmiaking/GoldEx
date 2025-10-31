using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.BarcodeInquiryAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.BarcodeInquiries;

public class BarcodeInquiriesByBarcodeSpecification : SpecificationBase<BarcodeInquiry>
{
    public BarcodeInquiriesByBarcodeSpecification(string barcode)
    {
        AddCriteria(x => x.Barcode == barcode);
    }
}