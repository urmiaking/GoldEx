using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByNumberSpecification : SpecificationBase<Invoice>
{
    public InvoicesByNumberSpecification(long invoiceNumber, InvoiceType invoiceType)
    {
        AddCriteria(x => x.InvoiceNumber == invoiceNumber);
        AddCriteria(x => x.InvoiceType == invoiceType);
    }
}