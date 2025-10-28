using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByMeltingBatchIdSpecification : SpecificationBase<Transaction>
{
    public TransactionsByMeltingBatchIdSpecification(MeltingBatchId batchId)
    {
        AddCriteria(x => x.MeltingBatchId == batchId);
        ApplyOrderBy(x => x.PostingDate);
    }
}