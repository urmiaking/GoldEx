using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByStoreIdSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByStoreIdSpecification(StoreId storeId)
    {
        AddCriteria(x => x.StoreId == storeId && x.CustomerId == null);
    }
}
