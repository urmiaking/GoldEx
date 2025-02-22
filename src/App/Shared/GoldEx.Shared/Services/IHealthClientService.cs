using GoldEx.Shared.DTOs.Application;

namespace GoldEx.Shared.Services;

public interface IHealthClientService
{
    Task<HealthCheckResponse> GetAsync(CancellationToken cancellationToken = default);
}