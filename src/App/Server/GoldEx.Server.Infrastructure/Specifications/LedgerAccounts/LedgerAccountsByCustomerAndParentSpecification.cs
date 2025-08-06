using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByCustomerAndParentSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByCustomerAndParentSpecification(CustomerId customerId, LedgerAccountId id)
    {
        AddCriteria(x => x.ParentAccountId == id);
        AddCriteria(x => x.CustomerId == customerId);
    }
}