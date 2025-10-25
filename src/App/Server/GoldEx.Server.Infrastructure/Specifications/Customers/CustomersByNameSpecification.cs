using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByNameSpecification : SpecificationBase<Customer>
{
    public CustomersByNameSpecification(string? customerName, CustomerType? type)
    {
        AddCriteria(x =>
            customerName == null ||
            x.FullName.Contains(customerName) ||
            x.NationalId == customerName ||
            (!string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber == customerName));

        if (type is not null)
            AddCriteria(x => x.CustomerType == type);
    }
}