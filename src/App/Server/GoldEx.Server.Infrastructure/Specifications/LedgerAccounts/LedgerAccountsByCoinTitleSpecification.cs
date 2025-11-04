using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByCoinTitleSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByCoinTitleSpecification(string coinTitle, LedgerAccountId coinParentLedgerAccountId)
    {
        AddCriteria(x => x.Title.Contains(coinTitle));
        AddCriteria(x => x.ParentAccountId == coinParentLedgerAccountId);
    }
}