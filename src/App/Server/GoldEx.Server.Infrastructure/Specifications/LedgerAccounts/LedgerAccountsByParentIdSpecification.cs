using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByParentIdSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByParentIdSpecification(LedgerAccountId parentId)
    {
        AddCriteria(x => x.ParentAccountId == parentId);
    }
}