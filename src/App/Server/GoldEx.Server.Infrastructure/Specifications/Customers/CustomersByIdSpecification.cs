using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByIdSpecification : SpecificationBase<Customer>
{
    public CustomersByIdSpecification(CustomerId customerId)
    {
        AddCriteria(x => x.Id == customerId);
        AddInclude(x => x.CreditLimitPriceUnit!);
    }
}