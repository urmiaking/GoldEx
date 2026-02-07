using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.MeltingBatchAggregate;

public readonly record struct MeltingBatchId(Guid Value);
public class MeltingBatch : EntityBase<MeltingBatchId>
{
    public int BatchNumber { get; private set; }

    public decimal TotalWeight { get; private set; }
    public GoldUnitType WeightUnitType { get; private set; }

    public CustomerId? AssayerId { get; private set; }
    public Customer? Assayer { get; private set; }

    private readonly List<MeltingBatchChangeLog> _changeLogs = [];
    public IReadOnlyList<MeltingBatchChangeLog> ChangeLogs => _changeLogs.OrderBy(x => x.DateTime).ToList();
    public IReadOnlyList<InventoryStock>? InventoryStocks { get; private set; }
    public IReadOnlyList<Transaction>? Transactions { get; private set; }

    public MeltingBatchStatus CurrentStatus => ChangeLogs.Any() ? ChangeLogs.MaxBy(x => x.DateTime)!.Status : MeltingBatchStatus.Melting;
    public DateTime CurrentDateTime => ChangeLogs.Any() ? ChangeLogs.MaxBy(x => x.DateTime)!.DateTime : DateTime.MinValue;

    private MeltingBatch(decimal totalWeight, GoldUnitType weightUnitType)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(totalWeight, 0);

        Id = new MeltingBatchId(Guid.NewGuid());
        TotalWeight = totalWeight;
        WeightUnitType = weightUnitType;
        _changeLogs = [MeltingBatchChangeLog.Create(MeltingBatchStatus.Melting)];
    }

#pragma warning disable CS8618
    private MeltingBatch() { }
#pragma warning restore CS8618

    public static MeltingBatch Create(decimal totalWeight, GoldUnitType weightUnitType)
    {
        return new MeltingBatch(totalWeight, weightUnitType);
    }

    private MeltingBatch AddChangeLog(MeltingBatchStatus status, string? description = null)
    {
        if (CurrentStatus >= status)
            throw new InvalidOperationException($"Melting batch is already in status {status}");

        _changeLogs.Add(MeltingBatchChangeLog.Create(status, description));

        return this;
    }

    public MeltingBatch SendToLab(CustomerId assayerId, string? description)
    {
        AssayerId = assayerId;
        return AddChangeLog(MeltingBatchStatus.SentToLab, description);
    }

    public MeltingBatch CompleteMelting(string? description = null)
    {
        return AddChangeLog(MeltingBatchStatus.Completed, description);
    }
}