using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByCustomerIdSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByCustomerIdSpecification(CustomerId? customerId)
    {
        if (customerId.HasValue)
        {
            AddCriteria(x => x.CustomerId == customerId);
        }
        else
        {
            AddCriteria(x => x.IsSystemAccount);
        }

    }
}