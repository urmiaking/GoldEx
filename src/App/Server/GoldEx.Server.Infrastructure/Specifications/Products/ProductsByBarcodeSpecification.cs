using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByBarcodeSpecification : SpecificationBase<Product>
{
    public ProductsByBarcodeSpecification(string barcode)
    {
        AddCriteria(x => x.Barcode == barcode);
        AddCriteria(x => x.ProductStatus == ProductStatus.Available);

        AddInclude(x => x.WagePriceUnit!);
    }
}