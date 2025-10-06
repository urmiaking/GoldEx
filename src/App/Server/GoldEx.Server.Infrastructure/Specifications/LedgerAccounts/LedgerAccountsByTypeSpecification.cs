using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByTypeSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByTypeSpecification(bool? isSystemAccount)
    {
        switch (isSystemAccount)
        {
            case false:
                AddCriteria(x => !x.IsSystemAccount);
                break;
            case true:
                AddCriteria(x => x.IsSystemAccount);
                break;
        }
    }
}