using GoldEx.Client.Offline.Domain.CheckpointAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Client.Offline.Infrastructure.Repositories.Abstractions;

public interface ICheckPointRepository : IRepository,
    ICreateRepository<Checkpoint>
{
    Task<Checkpoint?> GetLastCheckPointAsync(string entityName, CancellationToken cancellationToken = default);
}