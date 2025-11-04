using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByProductIdSpecification : SpecificationBase<Invoice>
{
    public InvoicesByProductIdSpecification(ProductId productId)
    {
        AddCriteria(x => x.ProductItems.Any(i => i.ProductId == productId));
        ApplyOrderByDescending(x => x.InvoiceDate);
    }
}