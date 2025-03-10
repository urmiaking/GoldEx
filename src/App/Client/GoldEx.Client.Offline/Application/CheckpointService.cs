using GoldEx.Client.Offline.Application.Abstractions;
using GoldEx.Client.Offline.Domain.CheckpointAggregate;
using GoldEx.Client.Offline.Infrastructure.Repositories.Abstractions;
using GoldEx.Sdk.Common.DependencyInjections;

namespace GoldEx.Client.Offline.Application;

[ScopedService]
public class CheckpointService(ICheckPointRepository repository) : ICheckpointService
{
    public Task<Checkpoint?> GetLastCheckPointAsync(string entityName, CancellationToken cancellationToken = default)
        => repository.GetLastCheckPointAsync(entityName, cancellationToken);

    public Task AddCheckPointAsync(Checkpoint checkPoint, CancellationToken cancellationToken = default)
        => repository.CreateAsync(checkPoint, cancellationToken);

    public Task UpdateAsync(Checkpoint checkPoint, CancellationToken cancellationToken = default)
        => repository.UpdateAsync(checkPoint, cancellationToken);
}