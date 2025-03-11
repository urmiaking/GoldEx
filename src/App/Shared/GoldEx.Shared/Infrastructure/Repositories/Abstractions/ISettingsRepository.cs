using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions.Base;

namespace GoldEx.Shared.Infrastructure.Repositories.Abstractions;

public interface ISettingsRepository<T> : IRepository,
    ICreateRepository<T>,
    IUpdateRepository<T> where T : SettingsBase
{
    Task<T?> GetAsync(SettingsId id, CancellationToken cancellationToken = default);
    Task<T?> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}