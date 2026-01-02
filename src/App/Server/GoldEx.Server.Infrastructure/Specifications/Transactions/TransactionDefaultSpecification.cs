using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionDefaultSpecification : SpecificationBase<Transaction>
{
    public TransactionDefaultSpecification(bool skipReversed = true)
    {
        if (skipReversed)
        {
            AddInclude(x => x.ReversedBy!);
            AddCriteria(x => x.ReverseTransactionId == null);
            AddCriteria(x => !x.ReversedBy!.Any());
        }
    }
}