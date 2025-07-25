using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByCustomerIdSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByCustomerIdSpecification(CustomerId customerId)
    {
        AddCriteria(b => b.CustomerId == customerId);
    }
}