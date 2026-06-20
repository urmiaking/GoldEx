using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.TransactionAggregate;

namespace GoldEx.Server.Domain.InventoryEntryAggregate;

public readonly record struct InventoryEntryId(Guid Value);
public class InventoryEntry : EntityBase<InventoryEntryId>, IStoreFiltered
{
    private InventoryEntry(StoreId storeId = default)
    {
        Id = new InventoryEntryId(Guid.CreateVersion7());
        StoreId = storeId;
    }

#pragma warning disable CS8618
    private InventoryEntry() { }
#pragma warning restore CS8618

    public static InventoryEntry Create(StoreId storeId = default) => new(storeId);

    public StoreId StoreId { get; private set; }
    public IReadOnlyList<InventoryStock>? InventoryStocks { get; private set; }
    public IReadOnlyList<Transaction>? Transactions { get; private set; }
}