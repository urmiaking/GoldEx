using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsParentSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsParentSpecification()
    {
        AddInclude(x => x.ParentAccount!.ParentAccount!.ParentAccount!);

        AddCriteria(x => x.ParentAccountId == null || x.ChildLedgerAccounts!.Any());
        AddInclude(x => x.PriceUnit!);
    }
}