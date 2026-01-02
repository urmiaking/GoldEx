using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByLedgerAccountIdSpecification : SpecificationBase<Transaction>
{
    public TransactionsByLedgerAccountIdSpecification(LedgerAccountId ledgerAccountId, bool skipReversed = true)
    {
        AddCriteria(x => x.LedgerAccountId == ledgerAccountId);

        if (skipReversed)
        {
            // TODO: need review
            AddCriteria(x => x.ReverseTransactionId == null);
        }

        ApplyOrderBy(x => x.PostingDate);
    }
}