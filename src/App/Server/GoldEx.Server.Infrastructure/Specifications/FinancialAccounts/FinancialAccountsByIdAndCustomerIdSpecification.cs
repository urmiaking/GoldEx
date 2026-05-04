using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByIdAndCustomerIdSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByIdAndCustomerIdSpecification(FinancialAccountId id, CustomerId customerId)
    {
        AddCriteria(x => x.Id == id && x.CustomerId == customerId);
    }
}