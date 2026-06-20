using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;

public class FinancialAccountsByStoreIdSpecification : SpecificationBase<FinancialAccount>
{
    public FinancialAccountsByStoreIdSpecification(StoreId storeId)
    {
        AddCriteria(x => x.StoreId == storeId && x.CustomerId == null && x.IsSystemAccount);
    }
}
