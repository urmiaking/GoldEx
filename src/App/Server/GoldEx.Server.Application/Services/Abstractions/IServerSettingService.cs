using GoldEx.Shared.DTOs.Settings;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerSettingService
{
    Task CreateAsync(CreateSettingRequest request, CancellationToken cancellationToken = default);
}