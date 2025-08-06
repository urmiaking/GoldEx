using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByLedgerAccountIdSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByLedgerAccountIdSpecification(LedgerAccountId ledgerAccountId)
    {
        AddCriteria(x => x.LedgerAccountId == ledgerAccountId);
    }
}