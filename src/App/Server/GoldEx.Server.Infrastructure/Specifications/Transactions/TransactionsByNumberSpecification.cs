using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByNumberSpecification : SpecificationBase<Transaction>
{
    public TransactionsByNumberSpecification(long number)
    {
        AddCriteria(x => x.Number == number);
    }
}