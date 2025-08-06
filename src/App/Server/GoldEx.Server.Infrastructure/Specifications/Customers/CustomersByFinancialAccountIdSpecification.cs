using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Customers;

public class CustomersByFinancialAccountIdSpecification : SpecificationBase<Customer>
{
    public CustomersByFinancialAccountIdSpecification(FinancialAccountId financialAccountId)
    {
        AddInclude(x => x.FinancialAccounts!);
        AddCriteria(x => x.FinancialAccounts != null 
                         && x.FinancialAccounts.Any(f => f.Id == financialAccountId));
    }
}