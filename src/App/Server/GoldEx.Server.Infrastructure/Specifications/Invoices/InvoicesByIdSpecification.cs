using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByIdSpecification : SpecificationBase<Invoice>
{
    public InvoicesByIdSpecification(InvoiceId id)
    {
        AddCriteria(x => x.Id == id);
    }
}