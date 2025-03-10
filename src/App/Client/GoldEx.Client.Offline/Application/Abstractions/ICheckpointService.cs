using GoldEx.Client.Offline.Domain.CheckpointAggregate;

namespace GoldEx.Client.Offline.Application.Abstractions;

public interface ICheckpointService
{
    Task<Checkpoint?> GetLastCheckPointAsync(string entityName, CancellationToken cancellationToken = default);
    Task AddCheckPointAsync(Checkpoint checkPoint, CancellationToken cancellationToken = default);
    Task UpdateAsync(Checkpoint checkPoint, CancellationToken cancellationToken = default);
}