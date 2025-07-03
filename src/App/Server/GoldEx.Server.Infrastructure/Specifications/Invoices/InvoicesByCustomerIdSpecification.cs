using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByCustomerIdSpecification : SpecificationBase<Invoice>
{
    public InvoicesByCustomerIdSpecification(CustomerId customerId)
    {
        AddCriteria(x => x.CustomerId == customerId);
    }
}