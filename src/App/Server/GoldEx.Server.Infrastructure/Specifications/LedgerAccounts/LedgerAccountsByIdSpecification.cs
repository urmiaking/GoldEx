using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByIdSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByIdSpecification(LedgerAccountId id)
    {
        AddCriteria(x => x.Id == id);
    }
}