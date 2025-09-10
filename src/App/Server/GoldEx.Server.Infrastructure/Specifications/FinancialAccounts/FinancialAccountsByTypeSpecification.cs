using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByTypeSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByTypeSpecification(FinancialAccountType financialAccountType, bool isSystemAccount)
    {
        AddCriteria(x => x.IsSystemAccount == isSystemAccount && x.AccountType == financialAccountType);
    }
}