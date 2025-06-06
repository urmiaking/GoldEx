using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoiceItemAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.InvoiceItems;

public class InvoiceItemsByInvoiceIdSpecification : SpecificationBase<InvoiceItem>
{
    public InvoiceItemsByInvoiceIdSpecification(InvoiceId invoiceId)
    {
        AddCriteria(x => x.InvoiceId == invoiceId);
    }
}