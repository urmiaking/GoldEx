using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.InventoryStockAggregate;

namespace GoldEx.Server.Domain.InventoryEntryAggregate;

public readonly record struct InventoryEntryId(Guid Value);
public class InventoryEntry : EntityBase<InventoryEntryId>
{
    private InventoryEntry()
    {
        Id = new InventoryEntryId(Guid.NewGuid());
    }

    public static InventoryEntry Create() => new();

    public IReadOnlyList<InventoryStock>? InventoryStocks { get; private set; }
}