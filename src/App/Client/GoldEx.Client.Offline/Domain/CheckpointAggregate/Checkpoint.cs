using GoldEx.Shared.Domain.Entities;

namespace GoldEx.Client.Offline.Domain.CheckpointAggregate;

public readonly record struct CheckpointId(Guid Value);
public class Checkpoint(string entityName) : EntityBase<CheckpointId>(new CheckpointId(Guid.NewGuid()))
{
    public string EntityName { get; private set; } = entityName;
    public DateTime DateTime { get; private set; } = DateTime.UtcNow;
}