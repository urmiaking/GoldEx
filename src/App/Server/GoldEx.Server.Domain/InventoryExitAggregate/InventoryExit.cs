using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.InventoryExitAggregate;

public readonly record struct InventoryExitId(Guid Value);
public class InventoryExit : EntityBase<InventoryExitId>
{
    public string? Description { get; private set; }
    public ExitReason ExitReason { get; private set; }
    public DateTime ExitDate { get; private set; }

    public IReadOnlyList<InventoryStock>? InventoryStocks { get; private set; }
    public IReadOnlyList<Transaction>? Transactions { get; private set; }

    private InventoryExit() { }

    public static InventoryExit Create(
        ExitReason exitReason,
        string? description = null,
        DateTime? exitDate = null)
    {
        return new InventoryExit
        {
            Id = new InventoryExitId(Guid.CreateVersion7()),
            ExitReason = exitReason,
            Description = description,
            ExitDate = exitDate ?? DateTime.Now
        };
    }

    public void SetDescription(string? description) => Description = description;
}