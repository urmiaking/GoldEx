using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Services;

namespace GoldEx.Client.Abstractions.LocalServices;

public interface ISettingLocalService : ISettingService
{
    Task CreateAsync(CreateSettingRequest request, CancellationToken cancellationToken = default);
}