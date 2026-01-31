using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByNameTypePhoneNumberSpecification : SpecificationBase<Customer>
{
    public CustomersByNameTypePhoneNumberSpecification(string fullName, string phoneNumber, CustomerType customerType)
    {
        AddCriteria(x => x.CustomerType == customerType && x.PhoneNumber == phoneNumber && x.FullName == fullName);
    }
}