using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsDefaultSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsDefaultSpecification(bool? isSystemAccount = false)
    {
        AddCriteria(x => isSystemAccount == null || x.IsSystemAccount == isSystemAccount);
        ApplyOrderByDescending(x => x.CreatedAt);
        AddInclude(x => x.PriceUnit!);
    }
}