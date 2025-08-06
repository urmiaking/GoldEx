using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;

public class LedgerAccountsByTitleSpecification : SpecificationBase<LedgerAccount>
{
    public LedgerAccountsByTitleSpecification(string title)
    {
        AddCriteria(ledgerAccount => ledgerAccount.Title == title);
    }
}