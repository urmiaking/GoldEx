using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Invoices;

public class InvoicesByCustomerIdSpecification : SpecificationBase<Invoice>
{
    public InvoicesByCustomerIdSpecification(CustomerId customerId)
    {
        AddCriteria(x => x.CustomerId == customerId);
    }

    public InvoicesByCustomerIdSpecification(CustomerId customerId, PriceUnitId priceUnitId, RequestFilter filter)
    {
        AddInclude(x => x.Customer!);
        AddInclude(x => x.InvoicePayments!);

        AddCriteria(x => x.CustomerId == customerId);

        AddCriteria(x => x.PriceUnitId == priceUnitId);

        if (!string.IsNullOrEmpty(filter.Search) && long.TryParse(filter.Search, out var number))
        {
            AddCriteria(x => x.InvoiceNumber == number);
        }
    }
}