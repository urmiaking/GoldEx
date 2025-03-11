using GoldEx.Shared.DTOs.Settings;

namespace GoldEx.Shared.Services;

public interface ISettingsClientService
{
    Task<GetSettingsResponse?> GetAsync(CancellationToken cancellationToken = default);
    Task<GetSettingsResponse?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateSettingsRequest request, CancellationToken cancellationToken = default);
    Task<GetSettingsResponse?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default);
}