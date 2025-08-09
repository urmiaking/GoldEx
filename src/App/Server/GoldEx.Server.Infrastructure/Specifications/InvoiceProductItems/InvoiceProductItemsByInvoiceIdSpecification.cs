using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InvoiceProductItems;

public class InvoiceProductItemsByInvoiceIdSpecification : SpecificationBase<InvoiceProductItem>
{
    public InvoiceProductItemsByInvoiceIdSpecification(InvoiceId invoiceId)
    {
        AddCriteria(x => x.InvoiceId == invoiceId);
    }
}