using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Products;

public class ProductsByBarcodeSpecification : SpecificationBase<Product>
{
    public ProductsByBarcodeSpecification(string barcode)
    {
        AddCriteria(x => x.Barcode == barcode);
    }
}