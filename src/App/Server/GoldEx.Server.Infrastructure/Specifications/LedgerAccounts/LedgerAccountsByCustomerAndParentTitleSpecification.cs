using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByCustomerAndParentTitleSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByCustomerAndParentTitleSpecification(CustomerId customerId, string parentTitle)
    {
        AddCriteria(x => x.CustomerId == customerId);
        AddCriteria(x => x.ParentAccount!.Title == parentTitle);

        AddInclude(x => x.ParentAccount!);
    }
}