using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByNationalIdSpecification : SpecificationBase<Customer>
{
    public CustomersByNationalIdSpecification(string nationalId, bool exactMatch = true)
    {
        AddCriteria(x => exactMatch ? x.NationalId == nationalId : x.NationalId.StartsWith(nationalId));
        AddInclude(x => x.CreditLimitPriceUnit!);
    }
}