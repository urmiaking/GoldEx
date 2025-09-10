using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByTypeSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByTypeSpecification(FinancialAccountType financialAccountType, Guid? customerId = null)
    {
        if (customerId is null)
        {
            AddCriteria(x => x.IsSystemAccount == true && x.AccountType == financialAccountType);
        }
        else
        {
            AddCriteria(x =>
                x.IsSystemAccount == false &&
                x.AccountType == financialAccountType &&
                x.CustomerId == new CustomerId(customerId.Value));
        }
    }
}