using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByNameAndTypeSpecification : SpecificationBase<Customer>
{
    public CustomersByNameAndTypeSpecification(string name, CustomerType type)
    {
        AddCriteria(x =>
            (string.IsNullOrEmpty(name) ||
             x.FullName.Contains(name) ||
             x.NationalId == name ||
             (!string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber == name))
            && x.CustomerType == type);
    }
}