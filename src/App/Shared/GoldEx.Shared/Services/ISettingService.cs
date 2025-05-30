using GoldEx.Shared.DTOs.Settings;

namespace GoldEx.Shared.Services;

public interface ISettingService
{
    /// <summary>
    /// Gets the current settings. If no settings are found, it returns null.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<GetSettingResponse?> GetAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(UpdateSettingRequest request, CancellationToken cancellationToken = default);
}