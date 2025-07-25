using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.BankAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.BankAccounts;

public class BankAccountsDefaultSpecification : SpecificationBase<BankAccount>
{
    public BankAccountsDefaultSpecification(bool? isSystemAccount = false)
    {
        AddCriteria(x => isSystemAccount == null || x.IsSystemAccount == isSystemAccount);
        ApplyOrderByDescending(x => x.CreatedAt);
    }
}