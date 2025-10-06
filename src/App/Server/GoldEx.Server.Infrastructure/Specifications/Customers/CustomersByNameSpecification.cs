using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByNameSpecification : SpecificationBase<Customer>
{
    public CustomersByNameSpecification(string? customerName)
    {
        AddCriteria(x =>
            customerName == null ||
            x.FullName.Contains(customerName) ||
            x.NationalId == customerName ||
            (!string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber == customerName));
    }
}