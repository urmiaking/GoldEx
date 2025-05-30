using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByNumberSpecification : SpecificationBase<Invoice>
{
    public InvoicesByNumberSpecification(long invoiceNumber)
    {
        AddCriteria(x => x.InvoiceNumber == invoiceNumber);
    }
}