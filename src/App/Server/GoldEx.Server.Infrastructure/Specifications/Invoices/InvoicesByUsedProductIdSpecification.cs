using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByUsedProductIdSpecification : SpecificationBase<Invoice>
{
    public InvoicesByUsedProductIdSpecification(ProductId productId)
    {
        AddCriteria(x => x.UsedProducts.Any(y => y.ProductId == productId));
    }
}