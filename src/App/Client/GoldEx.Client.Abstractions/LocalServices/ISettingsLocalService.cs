using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface ISettingsLocalService : ISettingsClientService
{
    Task CreateAsync(CreateSettingsRequest request, CancellationToken cancellationToken = default);
}