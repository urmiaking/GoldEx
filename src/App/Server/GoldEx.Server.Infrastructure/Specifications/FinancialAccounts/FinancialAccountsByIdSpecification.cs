using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByIdSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByIdSpecification(FinancialAccountId id)
    {
        AddCriteria(x => x.Id == id);
        AddInclude(x => x.LedgerAccount!); 
    }
}