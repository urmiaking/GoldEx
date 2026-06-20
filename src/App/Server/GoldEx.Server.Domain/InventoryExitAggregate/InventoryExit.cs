using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.InventoryExitAggregate;

public readonly record struct InventoryExitId(Guid Value);
public class InventoryExit : EntityBase<InventoryExitId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }
    public string? Description { get; private set; }
    public ExitReason ExitReason { get; private set; }
    public DateTime ExitDate { get; private set; }

    public IReadOnlyList<InventoryStock>? InventoryStocks { get; private set; }
    public IReadOnlyList<Transaction>? Transactions { get; private set; }

    private InventoryExit() { }

    public static InventoryExit Create(
        ExitReason exitReason,
        string? description = null,
        DateTime? exitDate = null,
        StoreId storeId = default)
    {
        return new InventoryExit
        {
            Id = new InventoryExitId(Guid.CreateVersion7()),
            ExitReason = exitReason,
            Description = description,
            ExitDate = exitDate ?? DateTime.Now,
            StoreId = storeId
        };
    }

    public void SetDescription(string? description) => Description = description;
}