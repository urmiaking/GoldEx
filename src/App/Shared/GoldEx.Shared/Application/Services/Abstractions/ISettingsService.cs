using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;

namespace GoldEx.Shared.Application.Services.Abstractions;

public interface ISettingsService<T> where T : SettingsBase
{
    Task CreateAsync(T settings, CancellationToken cancellationToken = default);
    Task UpdateAsync(T settings, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(SettingsId id, CancellationToken cancellationToken = default);
    Task<T?> GetAsync(CancellationToken cancellationToken = default);
    Task<T?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}