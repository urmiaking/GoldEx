using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.BankAccountAggregate;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.BankAccounts;

public class BankAccountsByCustomerIdSpecification : SpecificationBase<BankAccount>
{
    public BankAccountsByCustomerIdSpecification(CustomerId customerId)
    {
        AddCriteria(b => b.CustomerId == customerId);
    }
}