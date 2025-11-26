using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Transactions;

public class TransactionsByInventoryEntryIdSpecification : SpecificationBase<Transaction>
{
    public TransactionsByInventoryEntryIdSpecification(InventoryEntryId id)
    {
        AddCriteria(x => x.InventoryEntryId == id);
    }
}