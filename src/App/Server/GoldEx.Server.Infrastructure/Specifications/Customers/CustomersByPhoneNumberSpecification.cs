using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByPhoneNumberSpecification : SpecificationBase<Customer>
{
    public CustomersByPhoneNumberSpecification(string phoneNumber, bool exactMatch = true)
    {
        AddCriteria(x => !string.IsNullOrEmpty(x.PhoneNumber) && (exactMatch ? x.PhoneNumber == phoneNumber : x.PhoneNumber.StartsWith(phoneNumber)));
        AddInclude(x => x.CreditLimitPriceUnit!);
    }
}