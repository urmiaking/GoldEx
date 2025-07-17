using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.BankAccountAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.BankAccounts;

public class BankAccountsByIdSpecification : SpecificationBase<BankAccount>
{
    public BankAccountsByIdSpecification(BankAccountId id)
    {
        AddCriteria(x => x.Id == id);
    }
}