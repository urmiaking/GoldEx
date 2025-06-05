using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByIdSpecification : SpecificationBase<Transaction>
{
    public TransactionsByIdSpecification(TransactionId id)
    {
        AddCriteria(x => x.Id == id);

        AddInclude(x => x.CreditUnit!);
        AddInclude(x => x.DebitUnit!);
    }
}